using System;

namespace SelectedTextSpeach.Data.Entities
{
    public struct BlobArtifactEntity
    {
        public string Project { get; }
        public string Branch { get; }
        public string Artifact { get; }
        public Uri Uri { get; }
        public int Size { get; }

        public BlobArtifactEntity(string project, string branch, string artifact, Uri uri, int size) : this()
        {
            Project = project;
            Branch = branch;
            Artifact = artifact;
            Uri = uri;
            Size = size;
        }
    }
}
