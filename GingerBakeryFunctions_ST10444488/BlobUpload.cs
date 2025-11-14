using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace GingerBakeryFunctions_ST10444488
{
    public static class BlobUpload
    {
        [FunctionName("UploadProductImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing image upload...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string productId = data?.ProductId;
            string fileName = data?.FileName;
            string fileData = data?.FileData;

            if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileData))
            {
                return new BadRequestObjectResult("Missing required fields.");
            }

            byte[] imageBytes = Convert.FromBase64String((string)fileData);

            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("productimages");
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            var blob = container.GetBlockBlobReference($"{productId}/{fileName}");
            blob.Properties.ContentType = "image/jpeg"; 
            await blob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);

            string blobUrl = blob.Uri.ToString();
            log.LogInformation($"Image uploaded to: {blobUrl}");

            return new JsonResult(new { imageUrl = blobUrl });
        }
    }
}
