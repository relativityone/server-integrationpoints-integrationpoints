using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IAuditManager
    {
        IRelativityAuditRepository RelativityAuditRepository { get; }
    }
}