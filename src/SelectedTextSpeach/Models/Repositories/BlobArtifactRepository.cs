using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SelectedTextSpeach.Data.Entities;

namespace SelectedTextSpeach.Models.Repositories
{
    internal class BlobArtifactRepository
    {
        private string blobStorageConnection;

        public BlobArtifactRepository(string blobStorageConnection)
        {
            this.blobStorageConnection = blobStorageConnection;
        }

        public async Task<BlobArtifactEntity[]> GetBlobArtifactsAsync(string containerName)
        {
            var storageClient = CloudStorageAccount.Parse(blobStorageConnection);
            var blobClient = storageClient.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            // Flatten all items
            var blobInformations = new List<BlobArtifactEntity>();
            // project
            var directories = await GetBlobItemsAsync<CloudBlobDirectory>(container, null);
            foreach (var directory in directories)
            {
                // branch
                var branches = await GetBlobItemsAsync<CloudBlobDirectory>(container, directory.Prefix);
                await Task.WhenAll(branches.Select(async xs =>
                {
                    // blob
                    var details = await GetBlobItemsAsync<CloudBlockBlob>(container, xs.Prefix);
                    foreach (var detail in details)
                    {
                        var projectName = directory.Prefix.Substring(0, directory.Prefix.Length - 1);
                        var branchName = xs.Prefix.Substring(directory.Prefix.Length, xs.Prefix.Length - directory.Prefix.Length - 1);
                        blobInformations.Add(new BlobArtifactEntity(projectName, branchName, detail.Name, detail.Uri, detail.StreamWriteSizeInBytes));
                    }
                }));
            };

            return blobInformations.ToArray();
        }

        /// <summary>
        /// T should be CloudBlockBlob or CloudBlobDirectory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="directoryName"></param>
        /// <param name="useFlatBlobListing"></param>
        /// <returns></returns>
        private async Task<List<T>> GetBlobItemsAsync<T>(CloudBlobContainer container, string directoryName, bool useFlatBlobListing = false) where T : IListBlobItem
        {
            var list = new List<T>();
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = await container.ListBlobsSegmentedAsync(directoryName, useFlatBlobListing, BlobListingDetails.None, 100, blobContinuationToken, null, null);
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (var item in results.Results)
                {
                    list.Add((T)item);
                }
            } while (blobContinuationToken != null);
            return list;
        }
    }
}
