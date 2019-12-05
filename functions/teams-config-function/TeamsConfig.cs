using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamsConfiguration.Activities.Groups;
using TeamsConfiguration.Activities.Teams;
using TeamsConfiguration.Services;

namespace TeamsConfiguration
{
    public static class TeamsConfig
    {
        [FunctionName(nameof(TeamsConfig))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ExecutionContext exContext)
        {
            var payload = context.GetInput<string>();
            var configuration = ConfigurationService.GetInstance(exContext?.FunctionAppDirectory).Configuration.Value;

            var input = new CreationResult
            {
                FullProjectTitle = "Project leadership",
                ProjectDescription = "This channel helps project leaders get organized",
                ProjectSiteUrl = (configuration["LIBRARIES_SOURCE"] as string),
                ProjectChannelLibTabsLibNames = (configuration["LIBRARIES"] as string).Split(new char[] { ',' }).ToList(),
            };
            input.GroupId = await context.CallActivityAsync<string>(nameof(EnsureAndUpdateGroupActivity), payload);
            await context.CallActivityAsync(nameof(EnableTeamActivity), input);
            var channelCreation = await context.CallActivityWithRetryAsync<Tuple<string, bool>>(nameof(CreateProjectChannelActivity), new RetryOptions(TimeSpan.FromSeconds(10), 10)
            {
                BackoffCoefficient = 2 //after creating the team not everything is available right away and some calls fail
            }, input);
            input.ProjectChannelId = channelCreation.Item1;
            await context.CallActivityAsync(nameof(AddSPLibProjectTabActivity), input);
        }
    }
}