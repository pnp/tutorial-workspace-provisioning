using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeamsConfiguration.Services;

namespace TeamsConfiguration.Activities.Teams
{
    public abstract class AddSPLibTabActivity
    {
        protected static readonly string AppDefId = "com.microsoft.teamspace.tab.files.sharepoint";
        protected static async Task AddConfiguredLibTabsToChannel(string groupId, string channelId, string targetSiteOrSiteCollectionUrl, GraphServiceClient client, TeamsTabService tabService, List<string> librariesNames, Regex executer)
        {
            bool validation(string sectionName, TeamsTab x) => x.DisplayName == sectionName;
            var hostNameAndPath = getSiteHostNameAndPath(executer, targetSiteOrSiteCollectionUrl);
            var site = await getTargetSite(client, hostNameAndPath);
            foreach (var libraryName in librariesNames)
            {
                var libraryServerRelativeUrl = await getTargetLibrary(client, libraryName, hostNameAndPath.Item1, site);
                if (!await tabService.DoesTabExist(groupId, channelId, (x) => validation(libraryName, x)) && !string.IsNullOrEmpty(libraryServerRelativeUrl))
                    await tabService.AddTab(groupId, channelId, libraryName, getTabConfiguration($"https://{hostNameAndPath.Item1}{hostNameAndPath.Item2}", $"/{WebUtility.UrlEncode(libraryName)}"));
            }
        }
        private static async Task<string> getTargetLibrary(GraphServiceClient client, string libraryName, string hostName, Site customerSite)
        {
            var library = (await client.Sites[customerSite.Id].Lists.Request().Filter($"displayName eq '{libraryName}'").GetAsync()).FirstOrDefault();
            if (library == null)
                return null;
            else
                return library.WebUrl.Replace($"https://{hostName}", "");
        }
        private static Task<Site> getTargetSite(GraphServiceClient client, Tuple<string, string> hostNameAndPath)
        {
            var customerSiteId = $"{hostNameAndPath.Item1}{(string.IsNullOrEmpty(hostNameAndPath.Item2) ? string.Empty : ":")}{hostNameAndPath.Item2}";
            return client.Sites[customerSiteId].Request().GetAsync();
        }
        private static Tuple<string, string> getSiteHostNameAndPath(Regex executer, string value)
        {
            var customerHubMatch = executer.Match(value);
            if (customerHubMatch.Success)
                return new Tuple<string, string>(customerHubMatch.Groups["hostname"].Value, customerHubMatch.Groups["path"].Value);
            else
                return null;
        }
        private static TeamsTabConfiguration getTabConfiguration(string siteUrl, string libraryServerRelativeUrl)
        {
            return new TeamsTabConfiguration
            {
                ContentUrl = $"{siteUrl}{libraryServerRelativeUrl}"
            };
        }
    }
}
