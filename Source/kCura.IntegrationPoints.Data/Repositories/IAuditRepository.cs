using Relativity.MassImport;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IAuditRepository
	{
		bool AuditExport(int appID, bool isFatalError, ExportStatistics exportStats);
	}
}
