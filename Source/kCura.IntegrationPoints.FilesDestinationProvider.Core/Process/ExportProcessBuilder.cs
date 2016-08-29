using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.ScheduleQueue.Core;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;
using Relativity;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportProcessBuilder : IExportProcessBuilder
	{
		private readonly IConfigFactory _configFactory;
		private readonly ICaseManagerFactory _caseManagerFactory;
		private readonly ICredentialProvider _credentialProvider;
		private readonly IExporterFactory _exporterFactory;
		private readonly IExportFileBuilder _exportFileBuilder;
		private readonly JobStatisticsService _jobStatisticsService;
		private readonly ICompositeLoggingMediator _loggingMediator;
		private readonly ISearchManagerFactory _searchManagerFactory;
		private readonly IUserMessageNotification _userMessageNotification;
		private readonly IUserNotification _userNotification;

		public ExportProcessBuilder(
			IConfigFactory configFactory,
			ICompositeLoggingMediator loggingMediator,
			IUserMessageNotification userMessageNotification,
			IUserNotification userNotification,
			ICredentialProvider credentialProvider,
			ICaseManagerFactory caseManagerFactory,
			ISearchManagerFactory searchManagerFactory,
			IExporterFactory exporterFactory,
			IExportFileBuilder exportFileBuilder,
			JobStatisticsService jobStatisticsService
		)
		{
			_configFactory = configFactory;
			_loggingMediator = loggingMediator;
			_userMessageNotification = userMessageNotification;
			_userNotification = userNotification;
			_credentialProvider = credentialProvider;
			_caseManagerFactory = caseManagerFactory;
			_searchManagerFactory = searchManagerFactory;
			_exporterFactory = exporterFactory;
			_exportFileBuilder = exportFileBuilder;
			_jobStatisticsService = jobStatisticsService;
		}

		public SharedLibrary.IExporter Create(ExportSettings settings, Job job)
		{
			var exportFile = _exportFileBuilder.Create(settings);
			PerformLogin(exportFile);
			PopulateExportFieldsSettings(exportFile, settings.SelViewFieldIds, settings.TextPrecedenceFieldsIds);
			var exporter = _exporterFactory.Create(exportFile);
			AttachHandlers(exporter);
			SubscribeToJobStatisticsEvents(job);
			return exporter;
		}

		private void PerformLogin(ExportFile exportFile)
		{
			IConfig config = _configFactory.Create();
			WinEDDS.Config.ProgrammaticServiceURL = config.WebApiPath;

			var cookieContainer = new CookieContainer();

			exportFile.CookieContainer = cookieContainer;
			exportFile.Credential = _credentialProvider.Authenticate(cookieContainer);
		}

		private void PopulateExportFieldsSettings(ExportFile exportFile, List<int> selectedViewFieldIds, List<int> selectedTextPrecedence)
		{
			using (var searchManager = _searchManagerFactory.Create(exportFile.Credential, exportFile.CookieContainer))
			{
				using (var caseManager = _caseManagerFactory.Create(exportFile.Credential, exportFile.CookieContainer))
				{
					PopulateCaseInfo(exportFile, caseManager);

					SetAllExportableFields(exportFile, searchManager);

					PopulateViewFields(exportFile, selectedViewFieldIds);
					PopulateTextPrecedenceFields(exportFile, selectedTextPrecedence);
				}
			}
		}

		private static void SetAllExportableFields(ExportFile exportFile, ISearchManager searchManager)
		{
			exportFile.AllExportableFields = searchManager.RetrieveAllExportableViewFields(exportFile.CaseInfo.ArtifactID, exportFile.ArtifactTypeID);
		}

		private static void PopulateCaseInfo(ExportFile exportFile, ICaseManager caseManager)
		{
			if (string.IsNullOrEmpty(exportFile.CaseInfo.DocumentPath))
			{
				exportFile.CaseInfo = caseManager.Read(exportFile.CaseInfo.ArtifactID);
			}
		}

		private static void PopulateViewFields(ExportFile exportFile, List<int> selectedViewFieldIds)
		{
			exportFile.SelectedViewFields = FilterFields(exportFile, selectedViewFieldIds);

			var fieldIdentifier = exportFile.SelectedViewFields.FirstOrDefault(field => field.Category == FieldCategory.Identifier);
			if (fieldIdentifier == null)
			{
				throw new Exception($"Cannot find field identifier in the selected field list:" +
									$" {string.Join("", "", exportFile.SelectedViewFields.Select(field => field.DisplayName))} of {exportFile.FilePrefix}");
			}

			exportFile.IdentifierColumnName = fieldIdentifier.DisplayName;
		}

		private static void PopulateTextPrecedenceFields(ExportFile exportFile, List<int> selectedTextPrecedence)
		{
			if (exportFile.ExportFullTextAsFile)
			{
				exportFile.ExportFullText = true;
				exportFile.SelectedTextFields = FilterFields(exportFile, selectedTextPrecedence);
			}
		}

		private static ViewFieldInfo[] FilterFields(ExportFile exportFile, List<int> fieldsIds)
		{
			return exportFile.AllExportableFields
			   .Where(x => fieldsIds.Any(fieldId => fieldId == x.AvfId))
			   .OrderBy(x =>
			   {
				   var index = fieldsIds.IndexOf(x.AvfId);
				   return (index < 0) ? int.MaxValue : index;
			   }).ToArray();
		}

		private void AttachHandlers(SharedLibrary.IExporter exporter)   
		{
			exporter.InteractionManager = _userNotification;
			_loggingMediator.RegisterEventHandlers(_userMessageNotification, exporter);
		}

		private void SubscribeToJobStatisticsEvents(Job job)
		{
			foreach (var loggingMediator in
				_loggingMediator.LoggingMediators.Where(loggingMediator => loggingMediator.GetType().GetInterfaces().Contains(typeof(IBatchReporter))))
			{
				_jobStatisticsService.Subscribe(loggingMediator as IBatchReporter, job);
			}
		}
	}
}