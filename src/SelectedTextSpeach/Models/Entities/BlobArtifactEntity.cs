using System;

namespace SelectedTextSpeach.Models.Entities
{
    public interface IArtifactEntity
    {
        string Project { set; get; }
        IBranchArtifactEntity[] BranchArtifactDetail { set; get; }
    }

    public interface IBranchArtifactEntity
    {
        string Branch { set; get; }
        IArtifactDetailEntity[] Artifact { set; get; }
    }

    public interface IArtifactDetailEntity
    {
        string Name { set; get; }
        Uri Uri { set; get; }
        int Size { set; get; }
    }

    public struct BlobArtifactEntity : IArtifactEntity
    {
        public BlobArtifactEntity(string project, IBranchArtifactEntity[] branchArtifactDetail) : this()
        {
            Project = project;
            BranchArtifactDetail = branchArtifactDetail;
        }

        public string Project { set; get; }
        public IBranchArtifactEntity[] BranchArtifactDetail { set; get; }
    }

    public struct BlobBranchArtifactEntity : IBranchArtifactEntity
    {
        public BlobBranchArtifactEntity(string branch, IArtifactDetailEntity[] artifact) : this()
        {
            Branch = branch;
            Artifact = artifact;
        }

        public string Branch { set; get; }
        public IArtifactDetailEntity[] Artifact { set; get; }
    }

    public struct BlobArtifactDetailEntity : IArtifactDetailEntity
    {
        public BlobArtifactDetailEntity(string name, Uri uri, int size) : this()
        {
            Name = name;
            Uri = uri;
            Size = size;
        }

        public string Name { set; get; }
        public Uri Uri { set; get; }
        public int Size { set; get; }
    }
}
