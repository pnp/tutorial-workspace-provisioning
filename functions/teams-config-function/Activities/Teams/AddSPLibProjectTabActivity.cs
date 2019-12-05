using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeamsConfiguration.Services;

namespace TeamsConfiguration.Activities.Teams
{
    public class AddSPLibProjectTabActivity : AddSPLibTabActivity
    {
        private static readonly Regex webExtraction = new Regex(@"https://(?<hostname>[^.]+.sharepoint.com)(?<path>/[^/]+/[^/]+/[^/]+)?");
        [FunctionName(nameof(AddSPLibProjectTabActivity))]
        public static async Task Run([ActivityTrigger] CreationResult input, ILogger log, ExecutionContext context)
        {
            log.LogInformation("creating library tabs for client");
            var client = GraphServiceClientFactory.GetInstance(context?.FunctionAppDirectory).Client.Value;
            var tabService = new TeamsTabService(client, AppDefId);
            await AddConfiguredLibTabsToChannel(input.GroupId, input.ProjectChannelId, input.ProjectSiteUrl, client, tabService, input.ProjectChannelLibTabsLibNames, webExtraction);
            log.LogInformation("created library tabs for client");
        }
    }
}
