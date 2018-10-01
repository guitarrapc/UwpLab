using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Reactive.Bindings;
using SelectedTextSpeach.Data.Repositories;
using SelectedTextSpeach.Models.Entities;
using Windows.Storage;
using Windows.System;

namespace SelectedTextSpeach.Models.UseCases
{
    public interface IBlobArtifactSummary
    {
        ReadOnlyReactivePropertySlim<IArtifactEntity> Artifacts { get; }
        ReadOnlyReactivePropertySlim<string> RequestFailedMessage { get; }
        ReadOnlyReactivePropertySlim<string> DownloadStatus { get; }
        ReadOnlyReactivePropertySlim<string> DownloadPath { get; }

        void CancelRequest();
        Task RequestHoloLensPackagesAsync(string blobConnectionString, string containerName);
        Task DownloadHoloLensPackagesAsync(string blobConnectionString, string containerName, string blobName, long length, string fileName);
        Task OpenFolderAsync();
        IArtifactEntity[] GetArtifactCache();
        IBranchArtifactEntity[] GetArtifactCache(string projectName);
        IArtifactDetailEntity[] GetArtifactCache(string projectName, string branchNam);
    }

    public class BlobArtifactUseCase : IBlobArtifactSummary
    {
        public ReadOnlyReactivePropertySlim<IArtifactEntity> Artifacts => blobArtifactSubject.ToReadOnlyReactivePropertySlim();
        private Subject<IArtifactEntity> blobArtifactSubject = new Subject<IArtifactEntity>();
        public ReadOnlyReactivePropertySlim<string> RequestFailedMessage => blobArtifactFailedSubject.ToReadOnlyReactivePropertySlim();
        private Subject<string> blobArtifactFailedSubject = new Subject<string>();
        public ReadOnlyReactivePropertySlim<string> DownloadStatus => downloadStatusSubject.ToReadOnlyReactivePropertySlim();
        private Subject<string> downloadStatusSubject = new Subject<string>();
        public ReadOnlyReactivePropertySlim<string> DownloadPath => downloadPath.ToReadOnlyReactivePropertySlim();
        private Subject<string> downloadPath = new Subject<string>();

        private ConcurrentBag<IArtifactEntity> caches = null;
        private BlobArtifactRepository repository = null;
        private static readonly string downloadDirectoryName = "tmp";

        public void CancelRequest()
        {
            repository?.Cancel();
        }

        public async Task DownloadHoloLensPackagesAsync(string blobConnectionString, string containerName, string blobName, long length, string fileName)
        {
            // await DownloadAsFileAsync(blobConnectionString, containerName, blobName, length, fileName);
            await DownloadAsStreamAsync(blobConnectionString, containerName, blobName, length, fileName);
        }

        private async Task DownloadAsFileAsync(string blobConnectionString, string containerName, string blobName, long length, string fileName)
        {
            repository = new BlobArtifactRepository(blobConnectionString);

            // download
            downloadStatusSubject.OnNext("Start downloading.");
            var result = await repository.DownloadBlobArtifactAsync(containerName, blobName, length);

            // prepare
            var folder = await ReadyFolderAsync(ApplicationData.Current.LocalFolder, downloadDirectoryName);
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            // write file
            downloadStatusSubject.OnNext("Write to file.");
            await FileIO.WriteBytesAsync(file, result);

            // unzip
            downloadStatusSubject.OnNext("Begin unzip.");
            var parent = Path.GetFileNameWithoutExtension(file.Path);
            var extractFolder = await ReadyFolderAsync(folder, parent);
            await UnzipAsync(file, extractFolder);

            // clean up
            downloadStatusSubject.OnNext("clean up.");
            await file.DeleteAsync(StorageDeleteOption.Default);

            // notification
            downloadStatusSubject.OnNext("Complete.");
            downloadPath.OnNext(extractFolder.Path);
        }

        private async Task DownloadAsStreamAsync(string blobConnectionString, string containerName, string blobName, long length, string fileName)
        {
            repository = new BlobArtifactRepository(blobConnectionString);

            var bytes = new byte[length];
            using (var stream = new MemoryStream(bytes))
            {
                // download
                downloadStatusSubject.OnNext("Start downloading.");
                await repository.DownloadBlobArtifactAsync(containerName, blobName, stream);

                // prepare
                var folder = await ReadyFolderAsync(ApplicationData.Current.LocalFolder, downloadDirectoryName);
                var parent = Path.GetFileNameWithoutExtension(fileName);
                var extractFolder = await ReadyFolderAsync(folder, parent);

                // unzip
                downloadStatusSubject.OnNext("Begin unzip.");
                Unzip<MemoryStream>(stream, extractFolder);

                // notification
                downloadStatusSubject.OnNext("Complete.");
                downloadPath.OnNext(extractFolder.Path);
            }
            bytes = null;
        }

        private void Unzip<T>(T stream, StorageFolder extractFolder) where T : Stream
        {
            // unzip with long name validation
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    // entry.name == "" means it's directory.
                    if (string.IsNullOrWhiteSpace(entry.Name))
                        continue;

                    // check file name length is over windows limitation
                    var path = Path.Combine(extractFolder.Path, entry.Name);
                    if (path.Length > 280)
                    {
                        var leastLength = 280 - path.Length - Path.GetExtension(path).Length;
                        var newpath = string.Join("", Path.GetFileNameWithoutExtension(entry.Name).Take(leastLength).ToArray());
                        downloadStatusSubject.OnNext($"{path} is too long name. {path.Length} length. shorten to {newpath}");
                        path = newpath;
                    }
                    entry.ExtractToFile(path, true);
                }
            }
        }

        private async Task UnzipAsync(StorageFile file, StorageFolder extractFolder)
        {
            using (var stream = await file.OpenStreamForReadAsync())
            {
                Unzip(stream, extractFolder);
            }
        }

        public async Task OpenFolderAsync()
        {
            var folder = ApplicationData.Current.LocalFolder;
            await Launcher.LaunchFolderAsync(folder);
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

        private async Task<StorageFolder> ReadyFolderAsync(StorageFolder root, string directory)
        {
            var folder = Directory.Exists(Path.Combine(root.Path, directory))
                ? await root.GetFolderAsync(directory)
                : await root.CreateFolderAsync(directory);
            return folder;
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
