using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GingerBakeryFunctions_ST10444488
{
    public class QueueTrigger
    {
        [FunctionName("WriteToQueue")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string message = await new StreamReader(req.Body).ReadToEndAsync();

            var queueClient = new QueueClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "orderqueue");
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(message);

            return new OkObjectResult("Message sent to Queue");
        }
    }
}
