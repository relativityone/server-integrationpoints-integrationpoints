using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class AuditManager : IAuditManager
    {
        public AuditManager(IRelativityAuditRepository relativityAuditRepository)
        {
            RelativityAuditRepository = relativityAuditRepository;
        }

        public IRelativityAuditRepository RelativityAuditRepository { get; }
    }
}