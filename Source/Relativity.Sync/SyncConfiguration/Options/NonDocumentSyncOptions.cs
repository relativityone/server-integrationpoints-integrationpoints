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
        /// <param name="rdoArtifactTypeId">Artifact Type ID of the source RDO</param>
        /// <param name="destinationRdoArtifactTypeId">Artifact Type ID of the destination RDO</param>
        public NonDocumentSyncOptions(int sourceViewArtifactId, int rdoArtifactTypeId, int destinationRdoArtifactTypeId)
        {
            SourceViewArtifactId = sourceViewArtifactId;
            RdoArtifactTypeId = rdoArtifactTypeId;
            DestinationRdoArtifactTypeId = destinationRdoArtifactTypeId;
        }

        /// <summary>
        /// Determines Artifact Type ID of the source RDO
        /// </summary>
        public int RdoArtifactTypeId { get; }

        /// <summary>
        /// Determines Artifact Type ID of the destination RDO
        /// </summary>
        public int DestinationRdoArtifactTypeId { get; }

        /// <summary>
        /// Determines the Artifact Id of the source objects view
        /// </summary>
        public int SourceViewArtifactId { get; }
    }
}
