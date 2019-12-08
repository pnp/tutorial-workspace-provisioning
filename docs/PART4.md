# Part 4 - Test the end-to-end Office 365 group creation

In the part 4 we test all the pieces together by running the whole creation process from the beginning.

## Create a new workspace request

1. In the Azure autmation runbook _New-Workspace_, enable logs. This way, you will be able to soo the PowerShell script execution steps. 

    ![Enable runbook logs](/images/enable_logs.png)

2. In the workspace requests list from the previously created SharePoint site, create a new item.

    ![Create new workspaces](/images/create_new_workspace.png)

3. Go to the _New-Workspace_ runbook and inspect. You should see a new instance is running.

    ![Runbook Logs](/images/runbook_status1.png)

4. Click on the running instance to see the provisioning status. You should see the status updated as well in the workspace requests list.

    ![Runbook Logs](/images/runbook_status2.png)

    ![Item provisioning status](/images/item_provisioning_status.png)

5. When the creation is done, the new URL is available in the request item:

    ![Item provisioning status Completed](/images/creation_completed.png)

6. By browsing the site you should see the current members for the group. You will notice the owner set in the request is not set as 'Owner' in the group. This is due to the default behavior of groupe creation. Actually, in the provisioning script, the Office 365 group is created under the Azure AD Application identity (due to API limitation). It means **when an Office 365 group is created using the Graph API, the owner corresponds automatically to the identiy of the one perfoming the call** (in this case, the Azure AD application). To specify an other owner, you need to set it manually in a separate operation and this update is not instantaneous and can take few minutes (due to the fact the permissions are synchronized across different services).

    ![Group membership](/images/group_membership.png)

## What happen if case of provisioning issues?

In the case where the provisioning goes wrong, you have several places to look at to identify the cause:

1. The Azure Function used to catch the SharePoint item creation event:

    ![Azure Function logs](/images/func_logd.png)

2. The Azure Logic app used to start the automation runbook:

    ![Logic app logs](/images/logicapp_logs.png)

3. The Azure automation runbook logs:

    ![Runbook logs](/images/runbook_logs.png)

### Create a manual flow

Sometimes, issues can occur for dummy reasons (timeout, service unavailable at this time, etc.). In this case, you would just have to restart the provisioning process of the same site to get it right. To do this, an administrator can easily create a Microsoft Flow on the requests list, calling the Azure Logic app and therefore, the provisioning job again on the same site.

1. In the workspace requests lsit, create a new Flow of type '_SharePoint - For a selected item_' and connect to the list:

    ![Flow step 1](/images/flow_step1.png)

2. Add an action _HTTP request_ and use the Logic App URL with the item ID as body parameter:

    ![Flow step 2](/images/flow_step2.png)

3. Now you can re-run the provisioning sequence on a existing site manually:

    ![Start Flow](/images/start_flow.png)


> Next part: [Part 5 - Setup end-user experience](./PART5.md)
