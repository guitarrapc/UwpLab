using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Reactive.Bindings;
using SelectedTextSpeach.Data.Entities;
using SelectedTextSpeach.Models.Repositories;

namespace SelectedTextSpeach.Models
{
    public interface IBlobArtifactModel
    {
        ReadOnlyReactiveProperty<BlobArtifactEntity[]> Artifacts { get; }
        Task RequestHoloLensPackagesAsync(string blobConnectionString, string containerName);
        BlobArtifactEntity[] GetInfo();
    }

    public class BlobArtifactModel : IBlobArtifactModel
    {
        public ReadOnlyReactiveProperty<BlobArtifactEntity[]> Artifacts => blobArtifactSubject.ToReadOnlyReactiveProperty();
        private Subject<BlobArtifactEntity[]> blobArtifactSubject = new Subject<BlobArtifactEntity[]>();

        private BlobArtifactEntity[] cache = null;

        public async Task RequestHoloLensPackagesAsync(string blobConnectionString, string containerName)
        {
            try
            {
                var repository = new BlobArtifactRepository(blobConnectionString);
                var entities = await repository.GetBlobArtifactsAsync(containerName);
                blobArtifactSubject.OnNext(entities);
                cache = entities;
            }
            catch (Exception ex)
            {
                //TODO: Error Handling
                System.Diagnostics.Debug.WriteLine(ex);
                blobArtifactSubject.OnNext(null);
            }
        }

        public BlobArtifactEntity[] GetInfo()
        {
            return cache;
        }
    }
}
