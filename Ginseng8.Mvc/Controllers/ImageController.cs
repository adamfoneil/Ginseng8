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
	public class ImageController : Controller
	{
		private readonly DataAccess _data;
		private readonly BlobStorage _blob;

		public ImageController(IConfiguration config)
		{
			_data = new DataAccess(config);
			_blob = new BlobStorage(config);
		}

		[HttpPost]		
		public async Task<JsonResult> Paste([FromForm]IFormFile file)
		{
			try
			{
				_data.Initialize(User, TempData);
				if (!_data.CurrentUser.OrganizationId.HasValue) throw new Exception("Current user is not associated with an organization.");

				string orgName = _data.CurrentOrg.Name;

				var client = _blob.GetClient();
				var container = client.GetContainerReference(orgName);
				await container.CreateIfNotExistsAsync();
				await container.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });

				string fileName = Path.GetFileName(file.FileName);
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