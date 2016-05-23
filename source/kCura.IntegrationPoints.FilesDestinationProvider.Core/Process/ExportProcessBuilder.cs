using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public class ExportProcessBuilder : IExportProcessBuilder
    {
        private readonly ILoggingMediator _loggingMediator;
        private readonly IUserMessageNotification _userMessageNotification;
        private readonly IUserNotification _userNotification;
		private readonly ICredentialProvider _credentialProvider;

		public ExportProcessBuilder(ILoggingMediator loggingMediator, IUserNotification userNotification,
            IUserMessageNotification userMessageNotification, ICredentialProvider credentialProvider)
        {
            _loggingMediator = loggingMediator;
            _userNotification = userNotification;
            _userMessageNotification = userMessageNotification;
			_credentialProvider = credentialProvider;

        }

        public IExporter Create(ExportSettings settings)
        {
            var exportFile = ExportFileHelper.CreateDefaultSetup(settings);
            PerformLogin(exportFile, _credentialProvider);
            PopulateExportFieldsSettings(exportFile, settings.SelViewFieldIds);
            var exporter = new ExporterWrapper(new Exporter(exportFile, new Controller()));
            AttachHandlers(exporter, _loggingMediator);
            return exporter;
        }

        private void AttachHandlers(IExporter exporter, ILoggingMediator loggingMediator)
        {
            exporter.InteractionManager = _userNotification;
            loggingMediator.RegisterEventHandlers(_userMessageNotification, exporter);
        }

        private void PerformLogin(ExportFile exportSettings, ICredentialProvider credentialProvider)
        {
            var cookieContainer = new CookieContainer();

			exportSettings.CookieContainer = cookieContainer;
			exportSettings.Credential = credentialProvider.Authenticate(cookieContainer);
		}

        private void PopulateExportFieldsSettings(ExportFile exportFile, List<int> selectedViewFieldIds)
        {
            using (var searchManager = new SearchManager(exportFile.Credential, exportFile.CookieContainer))
            using (var caseManager = new CaseManager(exportFile.Credential, exportFile.CookieContainer))
            {
            	PopulateCaseInfo(exportFile, caseManager);
                PopulateViewFields(exportFile, selectedViewFieldIds, searchManager);
            }
        }

        private static void PopulateViewFields(ExportFile exportFile, List<int> selectedViewFieldIds,
            SearchManager searchManager)
        {
            exportFile.AllExportableFields =
                searchManager.RetrieveAllExportableViewFields(exportFile.CaseInfo.ArtifactID, exportFile.ArtifactTypeID);

            exportFile.SelectedViewFields = exportFile.AllExportableFields
                .Where(item => selectedViewFieldIds.Any(selViewFieldId => selViewFieldId == item.FieldArtifactId))
                .ToArray();
        }

        private static void PopulateCaseInfo(ExportFile exportFile, CaseManager caseManager)
        {
            if (string.IsNullOrEmpty(exportFile.CaseInfo.DocumentPath))
            {
                exportFile.CaseInfo = caseManager.Read(exportFile.CaseInfo.ArtifactID);
            }
        }
    }
}