using System;
using System.Threading.Tasks;

namespace SelectedTextSpeach.Models.UseCases
{
    public interface IBlobArtifactDownload
    {
        void Cancel();
        Task DownloadArtifact(string blobConnectionString, string containerName, string name);
    }

    public class BlobArtifactDownloadUseCase : IBlobArtifactDownload
    {
        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public Task DownloadArtifact(string blobConnectionString, string containerName, string name)
        {
            throw new NotImplementedException();
        }
    }
}
