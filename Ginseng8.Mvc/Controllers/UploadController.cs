using Ginseng.Models;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
				CloudBlockBlob blob = await UploadInnerAsync(file, folderName, id);

				return Json(new { link = _blob.GetUrlWithSas(blob) });
			}
			catch (Exception exc)
			{
				return Json(exc);
			}
		}

		private async Task<CloudBlockBlob> UploadInnerAsync(IFormFile file, string folderName, int id)
		{
			_data.Initialize(User, TempData);
			if (!_data.CurrentUser.OrganizationId.HasValue) throw new Exception("Current user is not associated with an organization.");

			string orgName = _data.CurrentOrg.Name;

			var client = _blob.GetClient();
			var container = client.GetContainerReference(orgName);
			await container.CreateIfNotExistsAsync();
			await container.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Off });

			string fileName = $"{folderName}/{id}/{Path.GetFileName(file.FileName)}";
			CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
			blob.Properties.ContentType = GetMimeType(fileName);

			using (var stream = file.OpenReadStream())
			{
				await blob.UploadFromStreamAsync(stream);
			}

			return blob;
		}

		public async Task<JsonResult> Attachment([FromForm]IFormFile file, string folderName, int id)
		{
			var blob = await UploadInnerAsync(file, folderName, id);

			var att = new Attachment(folderName)
			{
				ObjectId = id,
				OrganizationId = _data.CurrentOrg.Id,
				Url = _blob.GetUrlWithSas(blob),
				DisplayName = Path.GetFileName(blob.Name)
			};
			await _data.TrySaveAsync(att);

			return Json(att);
		}

		/// <summary>
		/// thanks to https://dotnetcoretutorials.com/2018/08/14/getting-a-mime-type-from-a-file-name-in-net-core/
		/// </summary>
		private string GetMimeType(string fileName)
		{
			const string defaultType = "application/octet-stream";
			var provider = new FileExtensionContentTypeProvider();
			return (provider.TryGetContentType(fileName, out string result)) ? result : defaultType;
		}
	}
}