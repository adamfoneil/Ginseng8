using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
	/// <summary>
	/// Stores json-serialized objects retrieved from API calls in a blob storage folder,
	/// with controls for throttling API calls to prevent use overages
	/// </summary>
	public abstract class IntegrationCache<T>
	{
		private readonly string _folderName;
		private readonly BlobStorage _blobStorage;
		private readonly string _orgName;

		private const string markerBlobName = "cache_time_marker.json";

		public IntegrationCache(IConfiguration config, string orgName, string folderName)
		{
			_folderName = folderName;
			_orgName = orgName;
			_blobStorage = new BlobStorage(config);
		}

		/// <summary>
		/// Implement this to return the items the cache manager
		/// </summary>		
		protected abstract Task<IEnumerable<T>> CallApiAsync();		

		/// <summary>
		/// How much time passes before we allow API to called again
		/// </summary>
		public abstract TimeSpan CallInterval { get; }

		public async Task<IEnumerable<T>> QueryAsync()
		{
			IEnumerable<T> results = null;

			if (await AllowApiCallAsync())
			{
				// enough time has passed since last API call
				results = await CallApiAsync();
				await CacheResultsAsync(results);				
			}
			else
			{
				// load from cache
				await _blobStorage.ListBlobs(_orgName, _folderName);
			}

			return results;
		}

		private async Task<bool> AllowApiCallAsync()
		{
			var lastCall = await GetLastApiCallTimeAsync();
			return (DateTime.UtcNow.Subtract(lastCall) > CallInterval);
		}

		private async Task CacheResultsAsync(IEnumerable<T> results)
		{
			foreach (T item in results)
			{

			}

			await SetLastApiCallTimeAsync();
		}

		private async Task SetLastApiCallTimeAsync()
		{
			var marker = await GetTimeMarkerBlobAsync();
			await marker.UploadTextAsync(JsonConvert.SerializeObject(DateTime.UtcNow));
		}

		private async Task<DateTime> GetLastApiCallTimeAsync()
		{
			var marker = await GetTimeMarkerBlobAsync();
			if (!(await marker.ExistsAsync())) return DateTime.MinValue;
			string dateTime = await marker.DownloadTextAsync();
			return JsonConvert.DeserializeObject<DateTime>(dateTime);
		}

		private async Task<CloudBlockBlob> GetTimeMarkerBlobAsync()
		{
			var container = await _blobStorage.GetOrgContainerAsync(_orgName);
			var blob = container.GetBlockBlobReference(_folderName + "/" + markerBlobName);
			blob.Properties.ContentType = "text/json";
			return blob;
		}
	}
}