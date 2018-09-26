using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Reactive.Bindings;
using SelectedTextSpeach.Data.Entities;
using SelectedTextSpeach.Models.Repositories;

namespace SelectedTextSpeach.Models.Usecase
{
    public interface IBlobArtifact
    {
        ReadOnlyReactivePropertySlim<IArtifactEntity[]> Artifacts { get; }
        Task RequestHoloLensPackagesAsync(string blobConnectionString, string containerName);
        IArtifactEntity[] GetArtifactCache();
        IBranchArtifactEntity[] GetArtifactCache(string projectName);
        IArtifactDetailEntity[] GetArtifactCache(string projectName, string branchNam);
    }

    public class BlobArtifactUsecase : IBlobArtifact
    {
        public ReadOnlyReactivePropertySlim<IArtifactEntity[]> Artifacts => blobArtifactSubject.ToReadOnlyReactivePropertySlim();
        private Subject<IArtifactEntity[]> blobArtifactSubject = new Subject<IArtifactEntity[]>();

        private IArtifactEntity[] cache = Array.Empty<IArtifactEntity>();

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
