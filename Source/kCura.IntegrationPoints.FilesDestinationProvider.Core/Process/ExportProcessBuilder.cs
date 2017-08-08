﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.ScheduleQueue.Core;
using kCura.WinEDDS;
using kCura.WinEDDS.Core.Model;
using kCura.WinEDDS.Core.Model.Export;
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
		private readonly IConfigFactory _configFactory;
		private readonly ICredentialProvider _credentialProvider;
		private readonly IExtendedExporterFactory _extendedExporterFactory;
		private readonly IExportFileBuilder _exportFileBuilder;
		private readonly JobStatisticsService _jobStatisticsService;
		private readonly IJobInfoFactory _jobHistoryFactory;
		private readonly IDirectoryHelper _dirHelper;
		private readonly IAPILog _logger;
		private readonly ICompositeLoggingMediator _loggingMediator;
		private readonly IUserMessageNotification _userMessageNotification;
		private readonly IUserNotification _userNotification;
		private readonly IExportServiceFactory _exportServiceFactory;

		public ExportProcessBuilder(IConfigFactory configFactory, ICompositeLoggingMediator loggingMediator,
			IUserMessageNotification userMessageNotification, IUserNotification userNotification,
			ICredentialProvider credentialProvider, IExtendedExporterFactory extendedExporterFactory,
			IExportFileBuilder exportFileBuilder, IHelper helper, JobStatisticsService jobStatisticsService,
			IJobInfoFactory jobHistoryFactory, IDirectoryHelper dirHelper, IExportServiceFactory exportServiceFactory)
		{
			_configFactory = configFactory;
			_loggingMediator = loggingMediator;
			_userMessageNotification = userMessageNotification;
			_userNotification = userNotification;
			_credentialProvider = credentialProvider;
			_extendedExporterFactory = extendedExporterFactory;
			_exportFileBuilder = exportFileBuilder;
			_jobStatisticsService = jobStatisticsService;
			_jobHistoryFactory = jobHistoryFactory;
			_dirHelper = dirHelper;
			_exportServiceFactory = exportServiceFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportProcessBuilder>();
		}

		public IExporter Create(ExportSettings settings, Job job)
		{
			try
			{
				LogCreatingExporter(settings);
				ExtendedExportFile exportFile = _exportFileBuilder.Create(settings);

				var exportDataContext = new ExportDataContext()
				{
					ExportFile = exportFile,
					Settings = settings
				};

				PerformLogin(exportFile);
				IExtendedServiceFactory serviceFactory = _exportServiceFactory.Create(exportDataContext);
				PopulateExportFieldsSettings(exportDataContext, serviceFactory);

				SetRuntimeSettings(exportFile, settings, job);
				IExporter exporter = _extendedExporterFactory.Create(exportDataContext, serviceFactory);
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

		private void SetRuntimeSettings(ExportFile exportFile, ExportSettings settings, Job job)
		{
			// TODO: move this to WinEDDS
			IJobInfo jobInfo = _jobHistoryFactory.Create(job);
			IDestinationFolderHelper destinationFolderHelper = new DestinationFolderHelper(jobInfo, _dirHelper);

			exportFile.FolderPath = destinationFolderHelper.GetFolder(settings);
			destinationFolderHelper.CreateDestinationSubFolderIfNeeded(settings, exportFile.FolderPath);
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

		private void PopulateExportFieldsSettings(ExportDataContext exportDataContext, IExtendedServiceFactory serviceFactory)
		{
			LogPopulatingFields();
			using (ISearchManager searchManager = serviceFactory.CreateSearchManager())
			{
				using (ICaseManager caseManager = serviceFactory.CreateCaseManager())
				{
					PopulateCaseInfo(exportDataContext.ExportFile, caseManager);
					SetRdoModeSpecificSettings(exportDataContext.ExportFile);
					SetAllExportableFields(exportDataContext.ExportFile, searchManager);

					PopulateViewFields(exportDataContext.ExportFile, exportDataContext.Settings.SelViewFieldIds.Select(item => item.Key).ToList());
					PopulateNativeFileNameViewFields(exportDataContext);
					PopulateTextPrecedenceFields(exportDataContext.ExportFile, exportDataContext.Settings.TextPrecedenceFieldsIds);
				}
			}
		}

		private void PopulateNativeFileNameViewFields(ExportDataContext exportDataContext)
		{
			IEnumerable<FieldDescriptorPart> fieldDescriptorParts = exportDataContext.Settings.FileNameParts != null
				? exportDataContext.Settings.FileNameParts.OfType<FieldDescriptorPart>()
				: Enumerable.Empty<FieldDescriptorPart>();
			if (fieldDescriptorParts.Any())
			{
				exportDataContext.ExportFile.SelectedNativesNameViewFields = FilterFields(exportDataContext.ExportFile,
					fieldDescriptorParts
						.Select(item => item.Value)
						.ToList())
					.ToList();
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