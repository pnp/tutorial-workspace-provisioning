using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TeamsConfiguration.Services;

namespace TeamsConfiguration.Activities.Groups
{
    public static class EnsureAndUpdateGroupActivity
    {
        [FunctionName(nameof(EnsureAndUpdateGroupActivity))]
        public static async Task<string> Run([ActivityTrigger] string title, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"getting group id for {title}");
            var client = GraphServiceClientFactory.GetInstance(context?.FunctionAppDirectory).Client.Value;

            var existingGroups = await client.Groups.Request().Filter($"displayName eq '{title}'").GetAsync();

            return existingGroups.FirstOrDefault()?.Id;
        }
    }
}
