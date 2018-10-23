using kCura.EDDS.WebAPI.AuditManagerBase;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.WinEDDS.Service.Export;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreAuditManager : IAuditManager
	{
		private readonly IHelper _helper;

		public CoreAuditManager(IHelper helper)
		{
			_helper = helper;
		}

		public bool AuditExport(int appID, bool isFatalError, ExportStatistics exportStats)
		{
			var repositoryFactory = new RepositoryFactory(_helper, _helper.GetServicesManager());
			IAuditRepository auditRepository = repositoryFactory.GetAuditRepository(appID);
			return auditRepository.AuditExport(exportStats.ToFoundationExportStatistics());
		}
	}
}