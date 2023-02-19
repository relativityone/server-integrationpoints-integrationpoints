using System;
using System.Xml.Linq;
using Relativity.API.Foundation;

namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
    public class AuditRecord : IAuditRecord
    {
        public AuditAction Action { get; }

        public int ArtifactID { get; }

        public XElement Details { get; }

        public TimeSpan? ExecutionTime { get; }

        public AuditRecord(int artifactID, AuditAction action, XElement details, TimeSpan? executionTime)
        {
            ArtifactID = artifactID;
            Action = action;
            Details = details;
            ExecutionTime = executionTime;
        }
    }
}
