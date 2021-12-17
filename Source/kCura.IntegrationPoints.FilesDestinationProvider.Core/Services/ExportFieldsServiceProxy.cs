using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Toggles;
using kCura.WinEDDS;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ExportFieldsServiceProxy : IExportFieldsService
	{
		private readonly IExportFieldsService _exportFieldsService;
		private readonly IAPILog _log;

		public ExportFieldsServiceProxy(IToggleProvider toggleProvider, IServicesMgr servicesMgr,
			IServiceManagerProvider serviceManagerProvider, IHelper helper)
		{
			_log = helper.GetLoggerFactory().GetLogger().ForContext<ExportFieldsServiceProxy>();

			_exportFieldsService = toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>()
				? (IExportFieldsService)new ExportFieldsService(servicesMgr)
				: new WebAPIExportFieldsService(serviceManagerProvider);
		}

		public FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID)
		{
			LogExecutionMethod(nameof(GetAllExportableFields));
			return _exportFieldsService.GetAllExportableFields(workspaceArtifactID, artifactTypeID);
		}

		public FieldEntry[] GetDefaultViewFields(int workspaceArtifactID, int viewArtifactID, int artifactTypeID, bool isProduction)
		{
			LogExecutionMethod(nameof(GetDefaultViewFields));
			return _exportFieldsService.GetDefaultViewFields(workspaceArtifactID, viewArtifactID, artifactTypeID, isProduction);
		}

		public FieldEntry[] GetAllExportableLongTextFields(int workspaceArtifactID, int artifactTypeID)
		{
			LogExecutionMethod(nameof(GetAllExportableLongTextFields));
			return _exportFieldsService.GetAllExportableLongTextFields(workspaceArtifactID, artifactTypeID);
		}

		public ViewFieldInfo[] RetrieveAllExportableViewFields(int workspaceID, int artifactTypeID, string correlationID)
		{
			LogExecutionMethod(nameof(RetrieveAllExportableViewFields));
			return _exportFieldsService.RetrieveAllExportableViewFields(workspaceID, artifactTypeID, correlationID);
		}

		private void LogExecutionMethod(string method)
        {
			_log.LogInformation("{type}.{method} was executed", _exportFieldsService.GetType(), method);
        }
	}
}
