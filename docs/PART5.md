# Part 5 - Setup end-user experience

The part 5 demonstrates how to setup a search experience based on workspaces metadata set at creation time using taxonomy values.

## PnP Modern Search Configuration - Search for workspaces with taxonomy filters

1. Download the **pnp-modern-search.sppkg** SPFx solution from [here](https://github.com/microsoft-search/pnp-modern-search/releases).

    ![PnP Modern Search](/images/pnp-modern-search.png)

1. In the workspace requests SharePoint site, add an app catalog by running the following commands:

        Connect-SPOService -Url https://<yourtenant>-admin.sharepoint.com
        Add-SPOSiteCollectionAppCatalog -Site https://<yourtenant>.sharepoint.com/sites/workspaces

1. Add the package to the app catalog:

    ![Add application to catalog](/images/add_app.png)

1. Add the application 'PnP Search Web Parts' in the site.

    ![Add application to site](/images/add_app_site.png)

1. Go to the site settings and the search schema. Create a new **'RefinableString'** (let's say _'RefinableString01'_) managed property and map it to the `PropertyBag_pnpCategory` crawled property. This managed property comes from the Office 365 SharePoint site property bag set by the provisioning script. It can take minutes to hours to see this property appear in your schema. 

    ![Crawled property mapping](/images/crawled_property_mapping.png)

1. Create a new SharePoint page and add the _'Search Box'_, _'Search results'_ and _'Search Refiners'_ Web Parts. Connect all the three Web Parts together.
    
1. In the _'Search results'_ Web Part, add this query as query template: `{searchTerms} contentclass:STS_Site AND Path:"https://<yourtenant>.sharepoint.com/sites/GRP*"`:

    ![Query template](/images/query_template.png)

1. In the _'Search Refiners'_ Web Part, configure refiners to use your _'RefinableString01'_ managed property.

    ![Query template](/images/configure_refiners.png)

1. Now you have a dedicated search page to browse provisioned Office 365 groups using taxonomy!

    ![Search page](/images/search_experience.png)

> Next part: [Part 6 - Setup Teams Configuration](./PART6.md)