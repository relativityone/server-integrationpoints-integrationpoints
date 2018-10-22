using kCura.EDDS.WebAPI.AuditManagerBase;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreAuditManager : IAuditManager
	{
		public bool AuditExport(int appID, bool isFatalError, ExportStatistics exportStats)
		{
			var auditRepository = new AuditRepository(); // TODO create and use factory
			return auditRepository.AuditExport(appID, exportStats.ToFoundationExportStatistics());
		}
	}
}