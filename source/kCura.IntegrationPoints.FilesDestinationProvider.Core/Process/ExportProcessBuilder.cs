using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public class ExportProcessBuilder : IExportProcessBuilder
    {
        private readonly ICaseManagerFactory _caseManagerFactory;
        private readonly ICredentialProvider _credentialProvider;
        private readonly IExporterFactory _exporterFactory;
        private readonly IExportFileBuilder _exportFileBuilder;
        private readonly ILoggingMediator _loggingMediator;
        private readonly ISearchManagerFactory _searchManagerFactory;
        private readonly IUserMessageNotification _userMessageNotification;
        private readonly IUserNotification _userNotification;

        public ExportProcessBuilder(ILoggingMediator loggingMediator, IUserMessageNotification userMessageNotification, IUserNotification userNotification,
            ICredentialProvider credentialProvider, ICaseManagerFactory caseManagerFactory, ISearchManagerFactory searchManagerFactory, IExporterFactory exporterFactory,
            IExportFileBuilder exportFileBuilder)
        {
            _loggingMediator = loggingMediator;
            _userMessageNotification = userMessageNotification;
            _userNotification = userNotification;
            _credentialProvider = credentialProvider;
            _caseManagerFactory = caseManagerFactory;
            _searchManagerFactory = searchManagerFactory;
            _exporterFactory = exporterFactory;
            _exportFileBuilder = exportFileBuilder;
        }

        public IExporter Create(ExportSettings settings)
        {
            var exportFile = _exportFileBuilder.Create(settings);
            PerformLogin(exportFile);
            PopulateExportFieldsSettings(exportFile, settings.SelViewFieldIds);
            var exporter = _exporterFactory.Create(exportFile);
            AttachHandlers(exporter);
            return exporter;
        }

        private void PerformLogin(ExportFile exportFile)
        {
            var cookieContainer = new CookieContainer();

            exportFile.CookieContainer = cookieContainer;
            exportFile.Credential = _credentialProvider.Authenticate(cookieContainer);
        }

        private void PopulateExportFieldsSettings(ExportFile exportFile, List<int> selectedViewFieldIds)
        {
            using (var searchManager = _searchManagerFactory.Create(exportFile.Credential, exportFile.CookieContainer))
            {
                using (var caseManager = _caseManagerFactory.Create(exportFile.Credential, exportFile.CookieContainer))
                {
                    PopulateCaseInfo(exportFile, caseManager);
                    PopulateViewFields(exportFile, selectedViewFieldIds, searchManager);
                }
            }
        }


        private static void PopulateCaseInfo(ExportFile exportFile, ICaseManager caseManager)
        {
            if (string.IsNullOrEmpty(exportFile.CaseInfo.DocumentPath))
            {
                exportFile.CaseInfo = caseManager.Read(exportFile.CaseInfo.ArtifactID);
            }
        }

        private static void PopulateViewFields(ExportFile exportFile, List<int> selectedViewFieldIds, ISearchManager searchManager)
        {
            exportFile.AllExportableFields = searchManager.RetrieveAllExportableViewFields(exportFile.CaseInfo.ArtifactID, exportFile.ArtifactTypeID);

			exportFile.SelectedViewFields = exportFile.AllExportableFields
				.Where(item => selectedViewFieldIds.Any(selViewFieldId => selViewFieldId == item.AvfId))
				.OrderBy(x =>
				{
					var index = selectedViewFieldIds.IndexOf(x.AvfId);
					return (index < 0) ? int.MaxValue : index;
				}).ToArray(); 
		}

        private void AttachHandlers(IExporter exporter)
        {
            exporter.InteractionManager = _userNotification;
            _loggingMediator.RegisterEventHandlers(_userMessageNotification, exporter);
        }
    }
}