using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Configuration class for RDO representing Destination Workspace tag
    /// </summary>
    public class DestinationWorkspaceOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DestinationWorkspaceOptions(Guid typeGuid, Guid nameGuid, Guid destinationWorkspaceNameGuid, Guid destinationWorkspaceArtifactIdGuid, Guid destinationInstanceNameGuid, Guid destinationInstanceArtifactIdGuid, Guid jobHistoryOnDocumentGuid, Guid destinationWorkspaceOnDocument)
        {
            TypeGuid = typeGuid;
            NameGuid = nameGuid;
            DestinationWorkspaceNameGuid = destinationWorkspaceNameGuid;
            DestinationWorkspaceArtifactIdGuid = destinationWorkspaceArtifactIdGuid;
            DestinationInstanceNameGuid = destinationInstanceNameGuid;
            DestinationInstanceArtifactIdGuid = destinationInstanceArtifactIdGuid;
            JobHistoryOnDocumentGuid = jobHistoryOnDocumentGuid;
            DestinationWorkspaceOnDocument = destinationWorkspaceOnDocument;
        }

        /// <summary>
        /// GUID for RDO type
        /// </summary>
        public Guid TypeGuid { get; private set; }

        /// <summary>
        /// GUID for Name field
        /// </summary>
        public Guid NameGuid { get; private set; }

        /// <summary>
        /// GUID for Destination Workspace Name field (fixed-length text, 400)
        /// </summary>
        public Guid DestinationWorkspaceNameGuid { get; private set; }

        /// <summary>
        /// GUID for Destination Workspace Artifact ID field (whole number)
        /// </summary>
        public Guid DestinationWorkspaceArtifactIdGuid { get; private set; }

        /// <summary>
        /// GUID for Destination Instance Name field (fixed-length text, 400)
        /// </summary>
        public Guid DestinationInstanceNameGuid { get; private set; }

        /// <summary>
        /// GUID for Destination Instance Artifact Id field (whole number)
        /// </summary>
        public Guid DestinationInstanceArtifactIdGuid { get; private set; }

        /// <summary>
        /// GUID for Job History field on Document (multiobject of JobHistory configured in JobHistory options)
        /// </summary>
        public Guid JobHistoryOnDocumentGuid { get; private set; }

        /// <summary>
        /// GUID for Destination Workspace field on Document (multiobject of Destination Workspace)
        /// </summary>
        public Guid DestinationWorkspaceOnDocument { get; private set; }
    }
}
