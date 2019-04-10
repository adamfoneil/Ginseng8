using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Controllers
{
	[Authorize]
	public class UploadController : Controller
	{
		private readonly DataAccess _data;
		private readonly BlobStorage _blob;

		public UploadController(IConfiguration config)
		{
			_data = new DataAccess(config);
			_blob = new BlobStorage(config);
		}

		[HttpPost]		
		public async Task<JsonResult> Image([FromForm]IFormFile file, string folderName, int id)
		{
			// critical help from https://stackoverflow.com/a/44538773/2023653
			try
			{
				_data.Initialize(User, TempData);
				if (!_data.CurrentUser.OrganizationId.HasValue) throw new Exception("Current user is not associated with an organization.");

				string orgName = _data.CurrentOrg.Name;

				var client = _blob.GetClient();
				var container = client.GetContainerReference(orgName);
				await container.CreateIfNotExistsAsync();
				await container.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });

				string fileName = $"{folderName}/{id}/{Path.GetFileName(file.FileName)}";
				CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

				using (var stream = file.OpenReadStream())
				{
					await blob.UploadFromStreamAsync(stream);
				}
					
				return Json(new { link = _blob.GetUrl(orgName, fileName) });
			}
			catch (Exception exc)
			{
				return Json(exc);
			}
		}
	}
}