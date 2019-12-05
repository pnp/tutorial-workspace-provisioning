using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TeamsConfiguration.Services
{
    public class TeamsTabService
    {
        private readonly string _appDefId;
        private readonly GraphServiceClient _client;
        public TeamsTabService(GraphServiceClient client, string appDefId)
        {
            _client = client;
            _appDefId = appDefId;
        }
        public async Task<bool> DoesTabExist(string groupId, string channelId, Func<TeamsTab, bool> validation)
        {
            var tabsCollection = await _client.Teams[groupId].Channels[channelId].Tabs.Request().Expand(nameof(TeamsTab.TeamsApp)).GetAsync();
            return tabsCollection.Any((x) => x.TeamsApp.Id == _appDefId && validation(x));
        }
        public async Task AddTab(string groupId, string channelId, string tabName, TeamsTabConfiguration configuration)
        {
            var tab = new TeamsTab
            {
                DisplayName = tabName,
                Configuration = configuration,
                AdditionalData = new Dictionary<string, object>
                {
                    { "teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{_appDefId}" }
                }
            };
            await _client.Teams[groupId].Channels[channelId].Tabs.Request().AddAsync(tab);
        }
    }
}
