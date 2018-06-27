using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class NullReporter : IBatchReporter
	{
		public event StatisticsUpdate OnStatisticsUpdate
		{
			add { }
			remove { }
		}

		public event BatchCompleted OnBatchComplete
		{
			add { }
			remove { }
		}

		public event BatchSubmitted OnBatchSubmit
		{
			add { }
			remove { }
		}

		public event BatchCreated OnBatchCreate
		{
			add { }
			remove { }
		}

		public event StatusUpdate OnStatusUpdate
		{
			add { }
			remove { }
		}

		public event JobError OnJobError
		{
			add { }
			remove { }
		}

		public event RowError OnDocumentError
		{
			add { }
			remove { }
		}
	}

	public class JobStatisticsService
	{
		private readonly IMessageService _messageService;
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IProviderTypeService _providerTypeService;
		private readonly IWorkspaceDBContext _context;
		private readonly TaskParameterHelper _helper;
		private readonly JobStatisticsQuery _query;
		private readonly IJobHistoryService _service;
		private readonly IAPILog _logger;
		private Job _job;

		private int _rowErrors;
		private readonly INativeFileSizeStatistics _nativeFileSizeStatistics;
		private readonly IImageFileSizeStatistics _imageFileSizeStatistics;
		private readonly IErrorFilesSizeStatistics _errorFilesSizeStatistics;

		public SourceConfiguration IntegrationPointSourceConfiguration { get; set; }
		public ImportSettings IntegrationPointImportSettings { get; set; }

		internal JobStatisticsService()
		{
		}

		public JobStatisticsService(JobStatisticsQuery query,
			TaskParameterHelper taskParameterHelper,
			IJobHistoryService service,
			IWorkspaceDBContext context,
			IHelper helper,
			INativeFileSizeStatistics nativeFileSizeStatistics,
			IImageFileSizeStatistics imageFileSizeStatistics,
			IErrorFilesSizeStatistics errorFilesSizeStatistics,
			IMessageService messageService, IIntegrationPointService integrationPointService, IProviderTypeService providerTypeService)
		{
			_query = query;
			_helper = taskParameterHelper;
			_service = service;
			_context = context;
			_nativeFileSizeStatistics = nativeFileSizeStatistics;
			_imageFileSizeStatistics = imageFileSizeStatistics;
			_errorFilesSizeStatistics = errorFilesSizeStatistics;
			_messageService = messageService;
			_integrationPointService = integrationPointService;
			_providerTypeService = providerTypeService;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<JobStatisticsService>();
		}

		public void Subscribe(IBatchReporter reporter, Job job)
		{
			if (reporter == null)
			{
				reporter = new NullReporter();
			}
			_job = job;
			reporter.OnStatusUpdate += StatusUpdate;
			reporter.OnStatisticsUpdate += OnStatisticsUpdate;
			reporter.OnBatchComplete += OnJobComplete;
			reporter.OnDocumentError += RowError;
		}

		private void OnStatisticsUpdate(double metadataThroughput, double fileThroughput)
		{
			string provider = GetProviderType(_job).ToString();

			var message = new JobApmThroughputMessage
			{
				Provider = provider,
				CorellationID = _helper.GetBatchInstance(_job).ToString(),
				UnitOfMeasure = "Byte(s)",
				JobID = _job.JobId.ToString(),
				WorkspaceID = ((IntegrationPointSourceConfiguration?.SourceWorkspaceArtifactId == 0)
			        ? IntegrationPointImportSettings?.CaseArtifactId
			        : IntegrationPointSourceConfiguration?.SourceWorkspaceArtifactId).GetValueOrDefault(),
				MetadataThroughput = metadataThroughput,
				FileThroughput = fileThroughput,
				CustomData = {["Provider"] = provider}
			};

			_messageService.Send(message);
		}

		private void RowError(string documentIdentifier, string errorMessage)
		{
			_rowErrors++;
		}

		public void SetIntegrationPointConfiguration(ImportSettings importSettings, SourceConfiguration sourceConfiguration)
		{
			IntegrationPointSourceConfiguration = sourceConfiguration;
			IntegrationPointImportSettings = importSettings;
		}

		private void OnJobComplete(DateTime start, DateTime end, int total, int errorCount)
		{
			string tableName = JobTracker.GenerateJobTrackerTempTableName(_job, _helper.GetBatchInstance(_job).ToString());
			JobStatistics stats = _query.UpdateAndRetrieveStats(tableName, _job.JobId, new JobStatistics { Completed = total, Errored = _rowErrors, ImportApiErrors = errorCount }, _job.WorkspaceID);
			_rowErrors = 0;

			long totalSize = CalculatePushedFilesSizeForJobHistory();

			Data.JobHistory historyRdo = _service.GetRdo(_helper.GetBatchInstance(_job));
			historyRdo.ItemsTransferred = stats.Imported > 0 ? stats.Imported : 0;
			historyRdo.ItemsWithErrors = stats.Errored;
			historyRdo.FilesSize = FormatFileSize(totalSize);
			_service.UpdateRdo(historyRdo);
		}

		private void StatusUpdate(int importedCount, int errorCount)
		{
			Update(_helper.GetBatchInstance(_job), importedCount, errorCount);
		}

		/// <summary>
		///     Multiple agents may be trying to access Job History data at the same time. Since agents can run across
		///     machines our database is used to act as a distributed lock coordinator. The mutex is managed by
		///     sp_getapplock and sp_releaseapplock.
		///     This prevents read -> read -> update -> update
		///     when we require read -> update -> read -> update.
		///     For more information see:
		///     sp_getapplock - https://msdn.microsoft.com/en-us/library/ms189823.aspx
		///     sp_releaseapplock - https://msdn.microsoft.com/en-us/library/ms178602.aspx
		/// </summary>
		public void Update(Guid identifier, int transferredItem, int erroredCount)
		{
			try
			{
				_context.BeginTransaction();
				EnableMutex(identifier);

				Data.JobHistory historyRdo = _service.GetRdo(_helper.GetBatchInstance(_job));
				if (transferredItem > 0)
				{
					historyRdo.ItemsTransferred += transferredItem;
				}

				historyRdo.ItemsWithErrors += erroredCount;
				_service.UpdateRdo(historyRdo);

				_context.CommitTransaction();
			}
			catch (Exception e)
			{
				LogUpdatingStatisticsError(e);
				// The mutex will be removed when the transaction is finalized
				_context.RollbackTransaction();
			}
		}

		/// <summary>
		///     This method must be called within a transaction
		/// </summary>
		private void EnableMutex(Guid identifier)
		{
			string enableJobHistoryMutex = $@"
				DECLARE @res INT
				EXEC @res = sp_getapplock
								@Resource = '{identifier}',
								@LockMode = 'Exclusive',
								@LockOwner = 'Transaction',
								@LockTimeout = 5000,
								@DbPrincipal = 'public'

				IF @res NOT IN (0, 1)
						BEGIN
							RAISERROR ( 'Unable to acquire mutex', 16, 1 )
						END";

			_context.ExecuteNonQuerySQLStatement(enableJobHistoryMutex);
		}

		private string FormatFileSize(long? bytes)
		{
			if (!bytes.HasValue || bytes == 0)
			{
				return "0 Bytes";
			}

			var k = 1024L;
			string[] sizes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

			var i = (long)Math.Floor(Math.Log((long)bytes) / Math.Log(k));
			return $"{bytes / Math.Pow(k, i):0.##} {sizes[i]}";
		}

		private long CalculatePushedFilesSizeForJobHistory()
		{
			if (!(IntegrationPointImportSettings?.ImportNativeFile).GetValueOrDefault(false) ||
				IntegrationPointSourceConfiguration == null)
			{
				return 0;
			}

			long filesSize = 0;

			switch (IntegrationPointSourceConfiguration.TypeOfExport)
			{
				case SourceConfiguration.ExportType.SavedSearch:

					filesSize = IntegrationPointImportSettings.ImageImport
						? _imageFileSizeStatistics.ForSavedSearch(IntegrationPointSourceConfiguration.SourceWorkspaceArtifactId,
							IntegrationPointSourceConfiguration.SavedSearchArtifactId)
						: _nativeFileSizeStatistics.ForSavedSearch(IntegrationPointSourceConfiguration.SourceWorkspaceArtifactId,
							IntegrationPointSourceConfiguration.SavedSearchArtifactId);
					break;

				case SourceConfiguration.ExportType.ProductionSet:
					filesSize = _imageFileSizeStatistics.ForProduction(IntegrationPointSourceConfiguration.SourceWorkspaceArtifactId,
						IntegrationPointSourceConfiguration.SourceProductionId);
					break;
			}

			long errorsFileSize =
				_errorFilesSizeStatistics.ForJobHistoryOmmitedFiles(IntegrationPointSourceConfiguration.SourceWorkspaceArtifactId,
					(int)_job.JobId);
			long copiedFilesFileSize = filesSize - errorsFileSize;

			return copiedFilesFileSize;
		}

		private ProviderType GetProviderType(Job job)
		{
			Data.IntegrationPoint integrationPoint = _integrationPointService.GetRdo(job.RelatedObjectArtifactID);
			ProviderType providerType = integrationPoint.GetProviderType(_providerTypeService);
			return providerType;
		}

		#region Logging

		private void LogUpdatingStatisticsError(Exception e)
		{
			_logger.LogError(e, "Error occurred during Job Statistics update.");
		}

		#endregion
	}
}