using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Reactive.Bindings;
using SelectedTextSpeach.Data.Repositories;
using SelectedTextSpeach.Models.Entities;

namespace SelectedTextSpeach.Models.UseCases
{
    public interface IBlobArtifact
    {
        ReadOnlyReactivePropertySlim<IArtifactEntity> Artifacts { get; }
        ReadOnlyReactivePropertySlim<string> RequestFailedMessage { get; }
        Task RequestHoloLensPackagesAsync(string blobConnectionString, string containerName);
        IArtifactEntity[] GetArtifactCache();
        IBranchArtifactEntity[] GetArtifactCache(string projectName);
        IArtifactDetailEntity[] GetArtifactCache(string projectName, string branchNam);
    }

    public class BlobArtifactUseCase : IBlobArtifact
    {
        public ReadOnlyReactivePropertySlim<IArtifactEntity> Artifacts => blobArtifactSubject.ToReadOnlyReactivePropertySlim();
        private Subject<IArtifactEntity> blobArtifactSubject = new Subject<IArtifactEntity>();
        public ReadOnlyReactivePropertySlim<string> RequestFailedMessage => blobArtifactFailedSubject.ToReadOnlyReactivePropertySlim();
        private Subject<string> blobArtifactFailedSubject = new Subject<string>();

        private IArtifactEntity[] cache = Array.Empty<IArtifactEntity>();

        public async Task RequestHoloLensPackagesAsync(string blobConnectionString, string containerName)
        {
            try
            {
                var repository = new BlobArtifactRepository(blobConnectionString)
                {
                    OnGetEachArtifact = artifact => blobArtifactSubject.OnNext(artifact),
                };
                var entities = await repository.GetBlobArtifactsAsync(containerName);
                cache = entities;
            }
            catch (Exception ex)
            {
                blobArtifactFailedSubject.OnNext($"Error: {ex.Message}");
            }
        }

        public IArtifactEntity[] GetArtifactCache()
        {
            if (!cache.Any())
            {
                return Array.Empty<IArtifactEntity>();
            }
            return cache;
        }

        public IBranchArtifactEntity[] GetArtifactCache(string projectName)
        {
            if (!cache.Any())
            {
                return Array.Empty<IBranchArtifactEntity>();
            }
            var result = cache.Where(x => x.Project == projectName)
                .SelectMany(x => x.BranchArtifactDetail)
                .ToArray();
            return result;
        }

        public IArtifactDetailEntity[] GetArtifactCache(string projectName, string branchName)
        {
            if (!cache.Any())
            {
                return Array.Empty<IArtifactDetailEntity>();
            }
            var result = cache.Where(x => x.Project == projectName)
                .SelectMany(x => x.BranchArtifactDetail)
                .Where(x => x.Branch == branchName)
                .SelectMany(x => x.Artifact)
                .ToArray();
            return result;
        }
    }
}
