using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamsConfiguration.Services;

namespace TeamsConfiguration.Activities.Teams
{
    public static class CreateProjectChannelActivity
    {
        [FunctionName(nameof(CreateProjectChannelActivity))]
        public static async Task<Tuple<string, bool>> Run([ActivityTrigger] CreationResult input, ILogger log, ExecutionContext context)
        {
            var client = GraphServiceClientFactory.GetInstance(context?.FunctionAppDirectory).Client.Value;
            var channels = await client.Teams[input.GroupId].Channels.Request().GetAsync();
            var projectChannel = channels.FirstOrDefault(x => x.DisplayName.Equals(input.FullProjectTitle, StringComparison.InvariantCultureIgnoreCase));

            if (projectChannel == null)
                return new Tuple<string, bool>(await CreateChannel(input, client), true);
            else
                return new Tuple<string, bool>(projectChannel.Id, false);
        }
        private static async Task<string> CreateChannel(CreationResult input, GraphServiceClient client)
        {
            var channel = await client.Teams[input.GroupId].Channels.Request().AddAsync(new Channel
            {
                DisplayName = input.FullProjectTitle,
                Description = input.ProjectDescription,
            });
            return channel.Id;
        }
    }
}
