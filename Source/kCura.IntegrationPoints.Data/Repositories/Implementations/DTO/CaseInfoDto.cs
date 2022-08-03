using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations.DTO
{
    internal class CaseInfoDto : ICaseInfoDto
    {
        public int ArtifactID { get; }
        public string Name { get; }
        public int MatterArtifactID { get; }
        public int StatusCodeArtifactID { get; }
        public bool EnableDataGrid { get; }
        public int RootFolderID { get; }
        public int RootArtifactID { get; }
        public string DownloadHandlerURL { get; }
        public bool AsImportAllowed { get; }
        public bool ExportAllowed { get; }
        public string DocumentPath { get; }

        public CaseInfoDto(
            int artifactId, 
            string name, 
            int matterArtifactId, 
            int statusCodeArtifactId, 
            bool enableDataGrid, 
            int rootFolderId, 
            int rootArtifactId, 
            string downloadHandlerUrl, 
            bool asImportAllowed, 
            bool exportAllowed, 
            string documentPath)
        {
            ArtifactID = artifactId;
            Name = name;
            MatterArtifactID = matterArtifactId;
            StatusCodeArtifactID = statusCodeArtifactId;
            EnableDataGrid = enableDataGrid;
            RootFolderID = rootFolderId;
            RootArtifactID = rootArtifactId;
            DownloadHandlerURL = downloadHandlerUrl;
            AsImportAllowed = asImportAllowed;
            ExportAllowed = exportAllowed;
            DocumentPath = documentPath;
        }
    }
}
