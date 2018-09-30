using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Reactive.Bindings;
using SelectedTextSpeach.Data.Repositories;
using SelectedTextSpeach.Models.Entities;

namespace SelectedTextSpeach.Models.UseCases
{
    public interface IBlobArtifactSummary
    {
        ReadOnlyReactivePropertySlim<IArtifactEntity> Artifacts { get; }
        ReadOnlyReactivePropertySlim<string> RequestFailedMessage { get; }

        void CancelRequest();
        Task RequestHoloLensPackagesAsync(string blobConnectionString, string containerName);
        IArtifactEntity[] GetArtifactCache();
        IBranchArtifactEntity[] GetArtifactCache(string projectName);
        IArtifactDetailEntity[] GetArtifactCache(string projectName, string branchNam);
    }

    public class BlobArtifactSummaryUseCase : IBlobArtifactSummary
    {
        public ReadOnlyReactivePropertySlim<IArtifactEntity> Artifacts => blobArtifactSubject.ToReadOnlyReactivePropertySlim();
        private Subject<IArtifactEntity> blobArtifactSubject = new Subject<IArtifactEntity>();
        public ReadOnlyReactivePropertySlim<string> RequestFailedMessage => blobArtifactFailedSubject.ToReadOnlyReactivePropertySlim();
        private Subject<string> blobArtifactFailedSubject = new Subject<string>();

        private ConcurrentBag<IArtifactEntity> caches = null;
        private BlobArtifactRepository repository = null;

        public void CancelRequest()
        {
            repository?.Cancel();
        }

        public async Task RequestHoloLensPackagesAsync(string blobConnectionString, string containerName)
        {
            try
            {
                caches = new ConcurrentBag<IArtifactEntity>();
                repository = new BlobArtifactRepository(blobConnectionString)
                {
                    OnGetEachArtifact = artifact =>
                    {
                        blobArtifactSubject.OnNext(artifact);
                        caches.Add(artifact);
                    },
                };
                var entities = await repository.ListBlobArtifactsAsync(containerName);
                repository = null;
            }
            catch (StorageException ex) when (ex.InnerException is TaskCanceledException)
            {
                blobArtifactFailedSubject.OnNext($"Canceled");
                PushCache();
            }
            catch (Exception ex)
            {
                blobArtifactFailedSubject.OnNext($"Error: {ex.Message}");
                PushCache();
            }
        }

        private void PushCache()
        {
            if (!caches.Any())
                return;
            foreach (var cache in caches)
            {
                blobArtifactSubject.OnNext(cache);
            }
        }

        public IArtifactEntity[] GetArtifactCache()
        {
            if (!caches.Any())
            {
                return Array.Empty<IArtifactEntity>();
            }
            return caches.ToArray();
        }

        public IBranchArtifactEntity[] GetArtifactCache(string projectName)
        {
            if (!caches.Any())
            {
                return Array.Empty<IBranchArtifactEntity>();
            }
            var result = caches.Where(x => x.Project == projectName)
                .SelectMany(x => x.BranchArtifactDetail)
                .ToArray();
            return result;
        }

        public IArtifactDetailEntity[] GetArtifactCache(string projectName, string branchName)
        {
            if (!caches.Any())
            {
                return Array.Empty<IArtifactDetailEntity>();
            }
            var result = caches.Where(x => x.Project == projectName)
                .SelectMany(x => x.BranchArtifactDetail)
                .Where(x => x.Branch == branchName)
                .SelectMany(x => x.Artifact)
                .ToArray();
            return result;
        }
    }
}
