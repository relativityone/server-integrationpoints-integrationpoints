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

		public ExportFieldsServiceProxy(IToggleProvider toggleProvider,
			IServicesMgr servicesMgr, IServiceManagerProvider serviceManagerProvider)
		{
			_exportFieldsService = toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>()
				? (IExportFieldsService)new ExportFieldsService(servicesMgr)
				: new WebAPIExportFieldsService(serviceManagerProvider);
		}

		public FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID)
		{
			return _exportFieldsService.GetAllExportableFields(workspaceArtifactID, artifactTypeID);
		}

		public FieldEntry[] GetDefaultViewFields(int workspaceArtifactID, int viewArtifactID, int artifactTypeID, bool isProduction)
		{
			return _exportFieldsService.GetDefaultViewFields(workspaceArtifactID, viewArtifactID, artifactTypeID, isProduction);
		}

		public FieldEntry[] GetAllExportableLongTextFields(int workspaceArtifactID, int artifactTypeID)
		{
			return _exportFieldsService.GetAllExportableLongTextFields(workspaceArtifactID, artifactTypeID);
		}

		public ViewFieldInfo[] RetrieveAllExportableViewFields(int workspaceID, int artifactTypeID, string correlationID)
		{
			return _exportFieldsService.RetrieveAllExportableViewFields(workspaceID, artifactTypeID, correlationID);
		}
	}
}
