using kCura.EDDS.WebAPI.AuditManagerBase;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class CoreAuditManager : IAuditManager
	{
		private readonly BaseServiceContext _baseServiceContext;

		public CoreAuditManager(BaseServiceContext baseServiceContext)
		{
			_baseServiceContext = baseServiceContext;
		}

		public bool AuditExport(int appID, bool isFatalError, ExportStatistics exportStats)
		{
			_baseServiceContext.AppArtifactID = appID;
			var massExportManager = new MassExportManager();
			return massExportManager.AuditExport(_baseServiceContext, isFatalError, exportStats.ToExportStatistics());
		}
	}
}