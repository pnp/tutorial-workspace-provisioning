using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Net;
using System.Threading.Tasks;
using TeamsConfiguration.Services;

namespace TeamsConfiguration.Activities.Teams
{
    public static class EnableTeamActivity
    {
        [FunctionName(nameof(EnableTeamActivity))]
        public static async Task Run([ActivityTrigger] CreationResult input, ILogger log, ExecutionContext context)
        {
            var client = GraphServiceClientFactory.GetInstance(context?.FunctionAppDirectory).Client.Value;
            if (!await DoesTeamExist(input, client))
                await EnableTeams(input, client);
        }
        private async static Task<bool> DoesTeamExist(CreationResult input, GraphServiceClient client)
        {
            try
            {
                var team = await client.Groups[input.GroupId].Team.Request().GetAsync();
                return team != null;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return false;
                else
                    throw;
            }
        }
        private static async Task EnableTeams(CreationResult input, GraphServiceClient client)
        {
            await client.Groups[input.GroupId].Team.Request().PutAsync(new Team
            {
                MemberSettings = new TeamMemberSettings
                {
                    AllowCreateUpdateChannels = true,
                    ODataType = null, // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/566#issuecomment-548057688
                },
                MessagingSettings = new TeamMessagingSettings
                {
                    AllowUserEditMessages = true,
                    AllowUserDeleteMessages = true,
                    ODataType = null, //https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/566#issuecomment-548057688
                },
                FunSettings = new TeamFunSettings
                {
                    AllowGiphy = true,
                    GiphyContentRating = GiphyRatingType.Strict,
                    ODataType = null, //https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/566#issuecomment-548057688
                },
                ODataType = null
            });
        }
    }
}
