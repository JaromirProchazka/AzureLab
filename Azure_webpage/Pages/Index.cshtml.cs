using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace MyWebApplication.Pages
{
	public class IndexModel : PageModel
	{
		// TODO: Hodnoty by bylo správné dát do konfigurace. Pro pøehlednost a mít vše na jednom místì za úèelem studia, nechávám tuto zjednodušenou variantu.
		private const string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=mystoraccountdemo;AccountKey=vSbs69SYRbXOsURUULN6F05EJ33/Hnw2qiEgPAU1gU2BFfK4vhUVjWBId83V8Yr4Zn1jGxVjMpMl+AStiOqPhA==;EndpointSuffix=core.windows.net";
		private const string AzureStorageContainerName = "mydemocontainer";

		public List<BlobItem> Blobs { get; set; } = new List<BlobItem>();

		public void OnGet()
		{
			Blobs = CreateClient().GetBlobs().ToList();
		}

		public async Task<FileStreamResult> OnGetDownloadBlob(string blobName)
		{
			var blobClient = CreateClient().GetBlobClient(blobName);
			var stream = await blobClient.DownloadStreamingAsync();
			return File(
				stream.Value.Content,
				stream.Value.Details.ContentType ?? "application/octet-stream",
				blobName);
		}

		private BlobContainerClient CreateClient()
		{
			return new Azure.Storage.Blobs.BlobContainerClient(AzureStorageConnectionString, AzureStorageContainerName);
		}
	}
}