using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public class ExportProcessBuilder : IExportProcessBuilder
    {
        private readonly ILoggingMediator _loggingMediator;
        private readonly IUserMessageNotification _userMessageNotification;
        private readonly IUserNotification _userNotification;

        public ExportProcessBuilder(ILoggingMediator loggingMediator, IUserNotification userNotification,
            IUserMessageNotification userMessageNotification)
        {
            _loggingMediator = loggingMediator;
            _userNotification = userNotification;
            _userMessageNotification = userMessageNotification;
        }

        public IExporter Create(ExportSettings settings)
        {
            var exportFile = ExportFileHelper.CreateDefaultSetup(settings);
            PerformLogin(exportFile);
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

        private void PerformLogin(ExportFile exportSettings)
        {
            exportSettings.CookieContainer = new CookieContainer();
            //exportSettings.Credential = WinEDDS.Api.LoginHelper.LoginWindowsAuth(exportSettings.CookieContainer);
            exportSettings.Credential = LoginHelper.LoginUsernamePassword("relativity.admin@kcura.com", "Test1234!",
                exportSettings.CookieContainer);
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