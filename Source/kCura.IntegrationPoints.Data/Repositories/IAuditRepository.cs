using ExportStatistics = Relativity.API.Foundation.ExportStatistics;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IAuditRepository
    {
        bool AuditExport(ExportStatistics exportStats, int contextUserId);
    }
}
