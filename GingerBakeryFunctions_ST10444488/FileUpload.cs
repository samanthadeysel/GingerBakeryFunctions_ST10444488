using Azure;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GingerBakeryFunctions_ST10444488
{
    public static class FileUpload
    {
        [FunctionName("UploadToFileShare")]
        public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
    ILogger log)
        {
            var file = req.Form.Files["file"];
            var shareClient = new ShareClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "contracts");
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(file.FileName);
            await fileClient.CreateAsync(file.Length);
            await fileClient.UploadRangeAsync(new HttpRange(0, file.Length), file.OpenReadStream());

            return new OkObjectResult("File uploaded to Azure Files");
        }
    }
}
