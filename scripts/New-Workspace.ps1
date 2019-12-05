Param
(
    [Parameter(Mandatory= $true)]
    $workspaceName,
    [Parameter(Mandatory= $true)]
    $workspaceDescription,
    [Parameter(Mandatory= $true)]
    $workspaceCategory,
    [Parameter(Mandatory= $true)]
    $workspaceOwner,
    [Parameter(Mandatory= $true)]
    $workspaceMembers
)

# Set Error Action preference
$ErrorActionPreference = 'Stop'

# Get variables
$certificatePassword = Get-AutomationVariable -Name "certificatePassword"
$storageConnectionString = Get-AutomationVariable -Name "storageConnectionString"
$fileShareName = Get-AutomationVariable -Name "fileShareName"
$provisioningTemplateFileName = Get-AutomationVariable -Name "provisioningTemplateFileName"
$spAdminSiteUrl = Get-AutomationVariable -Name 'spTenantUrl'
$appId = Get-AutomationVariable -Name 'appId'
$appSecret = Get-AutomationVariable -Name 'appSecret'
$aadDomain = Get-AutomationVariable -Name 'aadDomain'

# Create unique name to avoid concurrent file access.
$tmpWorkingFolderName = "C:\" + [System.IO.Path]::GetRandomFileName()

Write-Verbose "`t Temporary working directory created: '$tmpWorkingFolderName'"

# Format certificate to be used with PnP PowerShell module
$cert = Get-AutomationCertificate -Name 'pnpCert'
$pfxCert = $cert.Export(3, $certificatePassword) # 3=Pfx
$certPath = Join-Path $tmpWorkingFolderName "pnpCert.pfx"
Set-Content -Value $pfxCert -Path $certPath -Force -Encoding Byte

# Get PnP provisioning template file from Azure storage
$storageContext = New-AzureStorageContext -ConnectionString $storageConnectionString

$templateFile = New-TemporaryFile
Get-AzureStorageFileContent -Context $storageContext -ShareName $fileShareName -Path "/$provisioningTemplateFileName" -Destination $templateFile -Force

# Get PnP template resources directory from the Azure storage
$azResourcesFolder = Get-AzureStorageFile -Context $storageContext -ShareName $fileShareName -Path "/resources"

# Copy folder to local since we can't use the remote file path directly 
$tempResourcesFolder = New-Item -ItemType Directory -Path $tmpWorkingFolderName -Name "resources" -Force
$tempResourcesFolderPath = $tempResourcesFolder.FullName;

Write-Verbose "`tTemporary resources folder created: '$tempResourcesFolderPath'"

Get-AzureStorageFile -Directory $azResourcesFolder | % { 
    $filePath = "/" + $azResourcesFolder.Name + "/" + $_.Name
    
    $resourceFile = New-TemporaryFile
    Get-AzureStorageFileContent -Context $storageContext -ShareName $fileShareName -Path $filePath -Force -Destination $resourceFile

    $tempResourceFile = Join-Path $tempResourcesFolderPath $_.Name
    Set-Content -Value ($resourceFile | Get-Content) -Path $tempResourceFile -Force -Encoding UTF8
}

# Determine the Office 365 group URL + Remove special characters
$normalizedName = (($workspaceName.ToLower() -replace ' ','-') -replace '[^\p{L}\p{Nd}/_/-]', '')

# Remove diacritics
$normalizedName = [Text.Encoding]::ASCII.GetString([Text.Encoding]::GetEncoding("Cyrillic").GetBytes($normalizedName))
$mailNickname = "GRP_$normalizedName"

if ("GRP_$normalizedName".Length -gt 64) {
    # Limit to 64 characters
    $mailNickname = $mailNickname.Substring(0,64) 
}

Write-Verbose "Connecting to SharePoint Online using Azure AD application"
# Connect with Azure AD app to get Graph permissions (do not need client certificate in this scenario)
$graphConnection = Connect-PnPOnline -AppId $appId -AppSecret $appSecret -AADDomain $aadDomain

# Chek if the Office 365 group already exist
$group = Get-PnPUnifiedGroup -Identity $mailNickname

# Output group information

# Format parameters received from the Logic App        
$ownerUpn = ($workspaceOwner -split "\|")[-1]
$members = @($ownerUpn)
$workspaceMembers | % { 
    if ($_.Claims) {
        $members += ($_.Claims -split "\|")[-1]
    }
}

if ($members.Length -gt 1) {
    $allMembers = $members -join ","
}

Write-Verbose "`tGroup members:`t`t$allMembers"
Write-Verbose "`tGroup owner:`t`t$ownerUpn"
Write-Verbose "`tGroup URL:`t`t$mailNickname"
Write-Verbose "`tGroup name:`t`t$workspaceName"
Write-Verbose "`tGroup description:`t`t$workspaceDescription"

if ($group.SiteUrl -eq $null) {

    # Create a new Office 365 group
    Write-Verbose "`tCreating new Office 365 group '$mailNickname'" 
    $group = New-PnPUnifiedGroup -DisplayName $workspaceName -Description $workspaceDescription -MailNickname $mailNickname -IsPrivate:$true -Owners $ownerUpn -Members $members
    
} else {
    Write-Warning "The Office 365 group '$mailNickname' already exists. Skipping creation..."
}
    
$groupSiteUrl = $group.SiteUrl
$groupMail = $group.Mail

# Set group members and owners. Using AAD app only will set this app as owner by default. We need to set the actual user manually 
Set-PnPUnifiedGroup -Identity $group -Owners $ownerUpn -Members $members

# Allow custom scripts temporary for the site collection
$adminConnection = Connect-PnPOnline -CertificatePath $certPath -CertificatePassword (ConvertTo-SecureString $certificatePassword -AsPlainText -Force) -Tenant $aadDomain -ClientId $appId -Url $spAdminSiteUrl -ReturnConnection
Set-PnPTenantSite -Url $groupSiteUrl -NoScriptSite:$false -Connection $adminConnection

# Format default taxonomy values for workspace

# "Category"
$termCategoryDefaults = @()
$workspaceCategory | % {
    $termCategoryDefaults += ("-1;#" + $_.Label + "|" + $_.TermGuid)
}

$siteConnection = Connect-PnPOnline -CertificatePath $certPath -CertificatePassword (ConvertTo-SecureString $certificatePassword -AsPlainText -Force) -Tenant $aadDomain -ClientId $appId -Url $groupSiteUrl -ReturnConnection

# Set group permissions settings. Needed to get property bag indexed properties to be actually crawled...(won't work otherwise)
$ownerGroup = Get-PnPGroup -AssociatedOwnerGroup -Connection $siteConnection
Add-PnPUserToGroup -LoginName $ownerUpn -Identity $ownerGroup -Connection $siteConnection

# Apply the PnP Provisioning template
Write-Verbose "`tApplying pnp workspace template to '$groupSiteUrl'" 
Apply-PnPProvisioningTemplate -ResourceFolder $tempResourcesFolderPath -Path $templateFile.FullName -Parameters @{"defaultCategory"=($termCategoryDefaults -join ";#")} -Connection $siteConnection

# Add site property bag values for classification to be able to refine through regular search
Write-Verbose "`tAdding property bag values for search"

$categoryProperties = @()
$workspaceCategory | % {
    $categoryProperties += ("L0|#" + $_.TermGuid + "|" + $_.Label)
}

$propertyBagValues = @{
    # This format will be used by the ModernSearch WebPart to filter sites using taxonomy values
    "PropertyBag_pnpCategory"=($categoryProperties -join ";");
    # The group mail is used to generate the invitation link from the search results if needed
    "PropertyBag_pnpGroupMail"=$groupMail;
    "PropertyBag_pnpSiteType"="WORKSPACE";
}

$propertyBagValues.Keys | % {
 Set-PnPPropertyBagValue -Key $_ -Value $propertyBagValues[$_] -Indexed -Connection $siteConnection
}

# Reindex the site
Request-PnPReIndexWeb -Connection $siteConnection

# Reset custom script setting
Set-PnPTenantSite -Url $groupSiteUrl -NoScriptSite:$true -Connection $adminConnection

# Output the response for the logic app
$response = [PSCustomObject]@{
    GroupUrl = $groupSiteUrl
}

Write-Output ( $response | ConvertTo-Json)

# Remove temp folder
Remove-Item -Path $tmpWorkingFolderName -Force -Confirm:$false -Recurse
Write-Verbose "`t Temporary working directory removed: '$tmpWorkingFolderName'"