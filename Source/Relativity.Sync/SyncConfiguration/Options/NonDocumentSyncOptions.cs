namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Represents non-document objects synchronization options
    /// </summary>
    public class NonDocumentSyncOptions
    {
        /// <summary>
        /// Creates instance of <see cref="NonDocumentSyncOptions"/> class
        /// </summary>
        /// <param name="sourceViewArtifactId">Source object view Artifact Id</param>
        /// <param name="rdoArtifactTypeId">Object type Artifact If</param>
        public NonDocumentSyncOptions(int sourceViewArtifactId, int rdoArtifactTypeId)
        {
            SourceViewArtifactId = sourceViewArtifactId;
            RdoArtifactTypeId = rdoArtifactTypeId;
        }

        /// <summary>
        /// Determine type of transferred objects
        /// </summary>
        public int RdoArtifactTypeId { get; }
        
        
        /// <summary>
        /// Determines the Artifact Id of the source objects view
        /// </summary>
        public int SourceViewArtifactId { get; }
    }
}