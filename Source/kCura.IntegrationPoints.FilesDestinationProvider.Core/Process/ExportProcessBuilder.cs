using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.ScheduleQueue.Core;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;
using Relativity;
using Relativity.API;
using IExporter = kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary.IExporter;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportProcessBuilder : IExportProcessBuilder
	{
		private readonly ICaseManagerFactory _caseManagerFactory;
		private readonly IConfigFactory _configFactory;
		private readonly ICredentialProvider _credentialProvider;
		private readonly IExporterFactory _exporterFactory;
		private readonly IExportFileBuilder _exportFileBuilder;
		private readonly JobStatisticsService _jobStatisticsService;
		private readonly IAPILog _logger;
		private readonly ICompositeLoggingMediator _loggingMediator;
		private readonly IManagerFactory<ISearchManager> _searchManagerFactory;
		private readonly IUserMessageNotification _userMessageNotification;
		private readonly IUserNotification _userNotification;

		public ExportProcessBuilder(
			IConfigFactory configFactory,
			ICompositeLoggingMediator loggingMediator,
			IUserMessageNotification userMessageNotification,
			IUserNotification userNotification,
			ICredentialProvider credentialProvider,
			ICaseManagerFactory caseManagerFactory,
			IManagerFactory<ISearchManager> searchManagerFactory,
			IExporterFactory exporterFactory,
			IExportFileBuilder exportFileBuilder,
			IHelper helper,
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
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportProcessBuilder>();
		}

		public IExporter Create(ExportSettings settings, Job job)
		{
			try
			{
				LogCreatingExporter(settings);
				var exportFile = _exportFileBuilder.Create(settings);
				PerformLogin(exportFile);
				PopulateExportFieldsSettings(exportFile, settings.SelViewFieldIds, settings.TextPrecedenceFieldsIds);
				var exporter = _exporterFactory.Create(exportFile);
				AttachHandlers(exporter);
				SubscribeToJobStatisticsEvents(job);
				return exporter;
			}
			catch (Exception e)
			{
				LogCreatingExporterError(e);
				throw;
			}
		}

		private void PerformLogin(ExportFile exportFile)
		{
			IConfig config = _configFactory.Create();
			WinEDDS.Config.ProgrammaticServiceURL = config.WebApiPath;

			LogPerformingLogging(config);

			var cookieContainer = new CookieContainer();

			exportFile.CookieContainer = cookieContainer;
			exportFile.Credential = _credentialProvider.Authenticate(cookieContainer);
		}

		private void PopulateExportFieldsSettings(ExportFile exportFile, List<int> selectedViewFieldIds, List<int> selectedTextPrecedence)
		{
			LogPopulatingFields();
			using (var searchManager = _searchManagerFactory.Create(exportFile.Credential, exportFile.CookieContainer))
			{
				using (var caseManager = _caseManagerFactory.Create(exportFile.Credential, exportFile.CookieContainer))
				{
					PopulateCaseInfo(exportFile, caseManager);
					SetRdoModeSpecificSettings(exportFile);
					SetAllExportableFields(exportFile, searchManager);

					PopulateViewFields(exportFile, selectedViewFieldIds);
					PopulateTextPrecedenceFields(exportFile, selectedTextPrecedence);
				}
			}
		}

		private void SetRdoModeSpecificSettings(ExportFile exportFile)
		{
			// In the "Export RDO" mode we need to re-assign ArtifactId with Workspace Root Folder
			if (exportFile.TypeOfExport == ExportFile.ExportType.AncestorSearch &&
				exportFile.ArtifactTypeID != (int)ArtifactType.Document)
			{
				exportFile.ArtifactID = exportFile.CaseInfo.RootFolderID;
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

		private void PopulateViewFields(ExportFile exportFile, List<int> selectedViewFieldIds)
		{
			exportFile.SelectedViewFields = FilterFields(exportFile, selectedViewFieldIds);

			exportFile.IdentifierColumnName = exportFile.SelectedViewFields.FirstOrDefault(field => field.Category == FieldCategory.Identifier)?.DisplayName;
			
			var fileTypeField = exportFile.AllExportableFields.FirstOrDefault(field => field.FieldType == FieldTypeHelper.FieldType.File);
			if (fileTypeField != null)
			{
				exportFile.FileField = new DocumentField(fileTypeField.DisplayName, fileTypeField.FieldArtifactId, (int)fileTypeField.FieldType, 
					(int)fileTypeField.Category, fileTypeField.FieldCodeTypeID, 0, fileTypeField.AssociativeArtifactTypeID, fileTypeField.IsUnicodeEnabled, null, fileTypeField.EnableDataGrid);
			}

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
					return index < 0 ? int.MaxValue : index;
				}).ToArray();
		}

		private void AttachHandlers(IExporter exporter)
		{
			LogAttachingEventHandlers();
			exporter.InteractionManager = _userNotification;
			_loggingMediator.RegisterEventHandlers(_userMessageNotification, exporter);
		}

		private void SubscribeToJobStatisticsEvents(Job job)
		{
			List<ILoggingMediator> loggingMediators =
				_loggingMediator.LoggingMediators.Where(loggingMediator => loggingMediator.GetType().GetInterfaces().Contains(typeof(IBatchReporter))).ToList();

			LogSubscribingToStatisticEvents(loggingMediators);

			foreach (var loggingMediator in loggingMediators)
			{
				_jobStatisticsService.Subscribe(loggingMediator as IBatchReporter, job);
			}
		}

		#region Logging

		private void LogCreatingExporter(ExportSettings settings)
		{
			_logger.LogInformation("Attempting to create SharedLibrary.IExporter for exporting {ExportType}.", settings.TypeOfExport);
		}

		private void LogPerformingLogging(IConfig config)
		{
			_logger.LogInformation("Connecting to WebAPI in IExporter using WebAPIPath: {WebAPIPath}.", config.WebApiPath);
		}

		private void LogPopulatingFields()
		{
			_logger.LogVerbose("Attempting to populate export fields.");
		}

		private void LogMissingIdentifierFieldError(string message)
		{
			_logger.LogError(message);
		}

		private void LogAttachingEventHandlers()
		{
			_logger.LogVerbose("Attaching event handlers to IExporter.");
		}

		private void LogSubscribingToStatisticEvents(List<ILoggingMediator> loggingMediators)
		{
			_logger.LogVerbose("Subscribing {MediatorsCount} logging mediator(s) to statistic service.", loggingMediators.Count);
		}

		private void LogCreatingExporterError(Exception e)
		{
			_logger.LogError(e, "Failed to create Exporter.");
		}

		#endregion
	}
}