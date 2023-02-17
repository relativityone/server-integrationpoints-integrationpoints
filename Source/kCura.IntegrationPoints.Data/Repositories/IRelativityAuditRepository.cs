using kCura.IntegrationPoints.Data.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IRelativityAuditRepository
    {
        /// <summary>
        /// Create an audit record of a RDO with the detail.
        /// </summary>
        /// <param name="artifactID">An artifact Id of the auditing Rdo</param>
        /// <param name="auditElement">A DTO represents the auditing information.</param>
        void CreateAuditRecord(int artifactID, AuditElement auditElement);
    }
}
