using System;
using kCura.IntegrationPoints.Common.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class JobStatisticsService : IJobStatisticsService
	{
		private Job _job;
		private int _rowErrors;
		private static object _lockToken = new object();

		private readonly IMessageService _messageService;
		private readonly IIntegrationPointProviderTypeService _integrationPointProviderTypeService;
		private readonly IWorkspaceDBContext _context;
		private readonly ITaskParameterHelper _helper;
		private readonly IJobStatisticsQuery _query;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IAPILog _logger;

		private readonly IFileSizesStatisticsService _fileSizeStatisticsService;

		private SourceConfiguration IntegrationPointSourceConfiguration { get; set; }
		private ImportSettings IntegrationPointImportSettings { get; set; }

		public JobStatisticsService(IJobStatisticsQuery query,
			ITaskParameterHelper taskParameterHelper,
			IJobHistoryService jobHistoryService,
			IWorkspaceDBContext context,
			IHelper helper,
			IFileSizesStatisticsService fileSizesStatisticsStatisticsService,
			IMessageService messageService,
			IIntegrationPointProviderTypeService integrationPointProviderTypeService)
		{
			_query = query;
			_helper = taskParameterHelper;
			_jobHistoryService = jobHistoryService;
			_context = context;
			_messageService = messageService;
			_integrationPointProviderTypeService = integrationPointProviderTypeService;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<JobStatisticsService>();

			_fileSizeStatisticsService = fileSizesStatisticsStatisticsService;
		}

		/// <summary>
		/// This constructor is used in our unit tests
		/// </summary>
		internal JobStatisticsService()
		{
		}

		public void Subscribe(IBatchReporter reporter, Job job)
		{
			_job = job;

			if (reporter != null)
			{
				reporter.OnStatusUpdate += StatusUpdate;
				reporter.OnStatisticsUpdate += OnStatisticsUpdate;
				reporter.OnBatchComplete += OnJobComplete;
				reporter.OnDocumentError += RowError;
			}
		}

		private void OnStatisticsUpdate(double metadataThroughput, double fileThroughput)
		{
			string provider = GetProviderType(_job).ToString();

			var message = new JobProgressMessage
			{
				Provider = provider,
				CorrelationID = _helper.GetBatchInstance(_job).ToString(),
				UnitOfMeasure = UnitsOfMeasureConstants.BYTES,
				JobID = _job.JobId.ToString(),
				WorkspaceID = ((IntegrationPointSourceConfiguration?.SourceWorkspaceArtifactId == 0)
					? IntegrationPointImportSettings?.CaseArtifactId
					: IntegrationPointSourceConfiguration?.SourceWorkspaceArtifactId).GetValueOrDefault(),
				MetadataThroughput = metadataThroughput,
				FileThroughput = fileThroughput,
				CustomData = { ["Provider"] = provider }
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
			Guid batchInstance = _helper.GetBatchInstance(_job);
			string tableName = JobTracker.GenerateJobTrackerTempTableName(_job, batchInstance.ToString());
			long totalSize = _fileSizeStatisticsService.CalculatePushedFilesSizeForJobHistory((int)_job.JobId, IntegrationPointImportSettings, IntegrationPointSourceConfiguration);

			lock(_lockToken)
			{ 
				using (new JobHistoryMutex(_context, batchInstance))
				{
					// TODO refactoring, command query separation
					JobStatistics stats = _query.UpdateAndRetrieveStats(tableName, _job.JobId, new JobStatistics { Completed = total, Errored = _rowErrors, ImportApiErrors = errorCount }, _job.WorkspaceID);
					UpdateJobHistory(batchInstance, stats, totalSize);
				}
			}

			_rowErrors = 0;
		}

		private void StatusUpdate(int importedCount, int errorCount)
		{
			Update(_helper.GetBatchInstance(_job), importedCount, errorCount);
		}

		public void Update(Guid identifier, int transferredItem, int erroredCount)
		{
			lock(_lockToken)
			{
				using (new JobHistoryMutex(_context, identifier))
				{
					UpdateJobHistory(transferredItem, erroredCount);
				}
			}
		}

		private void UpdateJobHistory(Guid batchInstance, JobStatistics stats, long totalSize)
		{
			try
			{
				Data.JobHistory historyRdo = _jobHistoryService.GetRdo(batchInstance);
				historyRdo.ItemsTransferred = stats.Imported > 0 ? stats.Imported : 0;
				historyRdo.ItemsWithErrors = stats.ImportApiErrors;
				historyRdo.FilesSize = FileSizeUtils.FormatFileSize(totalSize);
				_jobHistoryService.UpdateRdo(historyRdo);
			}
			catch (Exception e)
			{
				LogUpdatingStatisticsError(e);
			}
		}

		private void UpdateJobHistory(int transferredItem, int erroredCount)
		{
			try
			{
				Data.JobHistory historyRdo = _jobHistoryService.GetRdo(_helper.GetBatchInstance(_job));
				int updatedNumberOfTransferredItems = (historyRdo.ItemsTransferred ?? 0) + transferredItem;
				historyRdo.ItemsTransferred = Math.Max(0, updatedNumberOfTransferredItems);
				historyRdo.ItemsWithErrors += erroredCount;
				_jobHistoryService.UpdateRdo(historyRdo);
			}
			catch (Exception e)
			{
				LogUpdatingStatisticsError(e);
			}
		}

		private ProviderType GetProviderType(Job job)
		{
			return _integrationPointProviderTypeService.GetProviderType(job.RelatedObjectArtifactID);
		}

		#region Logging

		private void LogUpdatingStatisticsError(Exception e)
		{
			_logger.LogError(e, "Error occurred during Job Statistics update.");
		}

		#endregion
	}
}