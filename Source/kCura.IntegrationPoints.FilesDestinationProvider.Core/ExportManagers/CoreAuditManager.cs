using kCura.EDDS.WebAPI.AuditManagerBase;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreAuditManager : IAuditManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public CoreAuditManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public bool AuditExport(int appID, bool isFatalError, ExportStatistics exportStats)
		{
			IAuditRepository auditRepository = _repositoryFactory.GetAuditRepository(appID);
			return auditRepository.AuditExport(exportStats.ToFoundationExportStatistics());
		}
	}
}
