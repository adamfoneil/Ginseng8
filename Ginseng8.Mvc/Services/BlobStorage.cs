using Ginseng.Mvc.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
	public class BlobStorageSettings
	{
		public string AccountName { get; set; }
		public string AccountKey { get; set; }
	}

	public class BlobStorage
	{
		private readonly IConfiguration _config;
		private readonly BlobStorageSettings _settings;

		public BlobStorage(IConfiguration config)
		{
			_config = config;
			_settings = config.GetSection("BlobStorage").Get<BlobStorageSettings>();
		}

		public string AccountName
		{
			get { return _settings.AccountName; }
		}

		public CloudStorageAccount GetAccount()
		{
			return new CloudStorageAccount(new StorageCredentials(AccountName, _settings.AccountKey), true);
		}

		public CloudBlobClient GetClient()
		{
			return GetAccount().CreateCloudBlobClient();
		}

		public static async Task<CloudBlobContainer> GetOrgContainerAsync(CloudBlobClient client, string orgName)
		{
			var container = client.GetContainerReference(orgName);
			await container.CreateIfNotExistsAsync();
			return container;
		}

		public async Task<CloudBlobContainer> GetOrgContainerAsync(string orgName)
		{
			var client = GetClient();
			return await GetOrgContainerAsync(client, orgName);
		}

		public string GetUrl(string orgName, string blobName)
		{
			return $"https://{AccountName}.blob.core.windows.net:443/{orgName}/{blobName}";
		}

		/// <summary>
		/// Returns blob's absolute URL (including SAS token if <c>includeSas == true</c>)
		/// </summary>
		/// <param name="blob">Existed Azure Storage Block Blob reference</param>
		/// <exception cref="ArgumentNullException">Thrown when <c>blob</c> parameter is null</exception>
		/// <returns>Blob's absolute URL (without SAS token)</returns>
		public string GetUrl(CloudBlockBlob blob)
		{
			if (blob == null) throw new ArgumentNullException(nameof(blob));
			return blob.Uri.AbsoluteUri;
		}

		/// <summary>
		/// Returns blob's SAS token
		/// </summary>
		/// <param name="blob">Existed Azure Storage Block Blob reference</param>
		/// <param name="permissions">Required access permissions</param>
		/// <param name="sasTokenLifetime">SAS token lifetime, when <c>null</c> expiration time is in 10 years</param>
		/// <exception cref="ArgumentNullException">Thrown when <c>blob</c> parameter is null</exception>
		/// <returns>Blob's SAS token</returns>
		public string GetSas(
			CloudBlockBlob blob,
			SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.Read,
			TimeSpan? sasTokenLifetime = null)
		{
			if (blob == null) throw new ArgumentNullException(nameof(blob));

			var policy = new SharedAccessBlobPolicy()
			{
				Permissions = permissions,
				SharedAccessExpiryTime = DateTimeOffset.UtcNow.Add(sasTokenLifetime ?? TimeSpan.FromDays(3650))
			};

			return blob.GetSharedAccessSignature(policy);
		}

		/// <summary>
		/// Returns blob's absolute URL including SAS token
		/// </summary>
		/// <param name="blob">Existed Azure Storage Block Blob reference</param>
		/// <param name="permissions">Required access permissions</param>
		/// <param name="sasTokenLifetime">SAS token lifetime</param>
		/// <exception cref="ArgumentNullException">Thrown when <c>blob</c> parameter is null</exception>
		/// <returns>Blob's absolute URL with SAS token </returns>
		public string GetUrlWithSas(
			CloudBlockBlob blob,
			SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.Read,
			TimeSpan? sasTokenLifetime = null)
		{
			if (blob == null) throw new ArgumentNullException(nameof(blob));
			return GetUrl(blob) + GetSas(blob, permissions, sasTokenLifetime);
		}

		public async Task<IEnumerable<T>> ListBlobsAsync<T>(string orgName, string prefix, Func<CloudBlockBlob, Task<T>> initializer, Func<CloudBlockBlob, bool> filter = null) where T : new()
		{
			var container = await GetOrgContainerAsync(orgName);
			BlobContinuationToken token = new BlobContinuationToken();
			List<T> results = new List<T>();
			do
			{
				var response = await container.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.None, null, token, null, null);
				token = response.ContinuationToken;
				var blobs = response.Results.OfType<CloudBlockBlob>();
				foreach (var blob in blobs)
				{
					if (filter?.Invoke(blob) ?? true)
					{
						var @object = await initializer.Invoke(blob);
						results.Add(@object);
					}
				}				
			} while (token != null);

			return results;
		}

		public async Task<IEnumerable<BlobInfo>> ListBlobsAsync(string orgName, string prefix)
		{
			return await ListBlobsAsync(orgName, prefix, async (blob) =>
			{
				return await Task.FromResult(new BlobInfo()
				{
					Uri = blob.Uri,
					Filename = Path.GetFileName(blob.Uri.ToString()),
					Length = blob.Properties.Length,
					LastModified = blob.Properties.LastModified
				});
			});
		}

		public async Task DeleteAsync(string orgName, string name)
		{
			var client = GetClient();
			var container = await GetOrgContainerAsync(client, orgName);
			var blob = container.GetBlockBlobReference(name);
			await blob.DeleteIfExistsAsync();
		}
	}
}