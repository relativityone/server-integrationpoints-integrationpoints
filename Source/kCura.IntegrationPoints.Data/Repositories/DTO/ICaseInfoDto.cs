namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
    public interface ICaseInfoDto
    {
        int ArtifactID { get; }
        string Name { get; }
        int MatterArtifactID { get; }
        int StatusCodeArtifactID { get; }
        bool EnableDataGrid { get; }
        int RootFolderID { get; }
        int RootArtifactID { get; }
        string DownloadHandlerURL { get; }
        bool AsImportAllowed { get; }
        bool ExportAllowed { get; }
        string DocumentPath { get; }
    }
}
