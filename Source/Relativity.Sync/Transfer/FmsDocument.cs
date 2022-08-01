namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Class that represents a single file transferred to ADLS
    /// </summary>
    internal sealed class FmsDocument
    {
        public int DocumentArtifactId { get; }
        public string FileName { get; }
        public string LinkForIAPI { get; }

        public FmsDocument(int documentArtifactId, string fileGuid, string pathForIapi)
        {
            DocumentArtifactId = documentArtifactId;
            FileName = fileGuid;
            LinkForIAPI = pathForIapi;
        }

    }
}
