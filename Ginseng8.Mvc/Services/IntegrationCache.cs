using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
	public enum LoadedFrom
	{
		Live,
		Cache
	}

	/// <summary>
	/// Stores json-serialized objects retrieved from API calls in a blob storage folder,
	/// with controls for throttling API calls to prevent use overages
	/// </summary>
	public abstract class IntegrationCache<T> where T : new()
	{
		private readonly string _folderName;
		private readonly BlobStorage _blobStorage;		

		private const string markerBlobName = "cache_time_marker.json";

		public IntegrationCache(IConfiguration config, string folderName)
		{
			_folderName = folderName;			
			_blobStorage = new BlobStorage(config);
		}

		/// <summary>
		/// Was data retrieved from cache (blob storage) or queried live?
		/// </summary>
		public LoadedFrom LoadedFrom { get; private set; }

		/// <summary>
		/// UTC time of last API call
		/// </summary>
		public DateTime LastApiCallDateTime { get; private set; }

		/// <summary>
		/// Implement this to return the T items from the API
		/// </summary>		
		protected abstract Task<IEnumerable<T>> ApiQueryAll(string orgName);

		/// <summary>
		/// How much time passes before we allow API to called again
		/// </summary>
		public abstract TimeSpan CallInterval { get; }

		/// <summary>
		/// When given a blob, how do we convert this to T?
		/// </summary>
		protected abstract Task<T> ConvertFromBlobAsync(CloudBlockBlob blob);

		/// <summary>
		/// How do we name the blobs from the incoming object?
		/// </summary>
		protected abstract string GetBlobName(T @object);

		/// <summary>
		/// Override this if you need to customize how serialization happens
		/// </summary>
		protected virtual string SerializeObject(T @object)
		{
			return JsonConvert.SerializeObject(@object);
		}

		public async Task<IEnumerable<T>> QueryAsync(string orgName)
		{
			IEnumerable<T> results = null;

			if (await AllowApiCallAsync(orgName))
			{
                await ClearCacheAsync(orgName);

				// enough time has passed since last API call
				results = await ApiQueryAll(orgName);

				// save results in blob storage so I can get them again without hitting the API
				var container = await _blobStorage.GetOrgContainerAsync(orgName);
				foreach (T item in results)
				{
					string blobName = GetBlobName(item);
					var blob = container.GetBlockBlobReference(_folderName + "/" + blobName);
					string content = SerializeObject(item);
					blob.Properties.ContentType = "text/json";
					await blob.UploadTextAsync(content);
				}
				await SetLastApiCallTimeAsync(orgName);
				LoadedFrom = LoadedFrom.Live;
			}
			else
			{
				// load from blob storage (except for the time marker)
				results = await _blobStorage.ListBlobsAsync(orgName, _folderName, 
					async (blob) => await ConvertFromBlobAsync(blob), 
					(blob) => !blob.Name.Equals(_folderName + "/" + markerBlobName));
				LoadedFrom = LoadedFrom.Cache;
			}

			return results;
		}

        private async Task ClearCacheAsync(string orgName)
        {
            var blobs = await _blobStorage.ListBlobsAsync(orgName, _folderName, (blob) => !blob.Name.Equals(_folderName + "/" + markerBlobName));
            foreach (var blob in blobs) await _blobStorage.DeleteAsync(orgName, _folderName + "/" + blob.Filename);
        }

        public async Task<T> GetBlobAsync(string orgName, T criteria)
        {
            string blobName = GetBlobName(criteria);
            var container = await _blobStorage.GetOrgContainerAsync(orgName);
            var blob = container.GetBlockBlobReference(_folderName + "/" + blobName);
            return await ConvertFromBlobAsync(blob);
        }

		private async Task<bool> AllowApiCallAsync(string orgName)
		{
			var lastCall = await GetLastApiCallTimeAsync(orgName);
			LastApiCallDateTime = lastCall;
			return (DateTime.UtcNow.Subtract(lastCall) > CallInterval);
		}

		private async Task SetLastApiCallTimeAsync(string orgName)
		{
			LastApiCallDateTime = DateTime.UtcNow;
			var marker = await GetTimeMarkerBlobAsync(orgName);
			await marker.UploadTextAsync(JsonConvert.SerializeObject(LastApiCallDateTime));
		}

		private async Task<DateTime> GetLastApiCallTimeAsync(string orgName)
		{
			var marker = await GetTimeMarkerBlobAsync(orgName);
			if (!(await marker.ExistsAsync())) return DateTime.MinValue;
			string dateTime = await marker.DownloadTextAsync();
			return JsonConvert.DeserializeObject<DateTime>(dateTime);
		}

		private async Task<CloudBlockBlob> GetTimeMarkerBlobAsync(string orgName)
		{
			var container = await _blobStorage.GetOrgContainerAsync(orgName);
			var blob = container.GetBlockBlobReference(_folderName + "/" + markerBlobName);
			blob.Properties.ContentType = "text/json";
			return blob;
		}
	}
}