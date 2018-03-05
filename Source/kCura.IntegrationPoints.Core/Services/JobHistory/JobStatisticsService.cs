using System;
using System.Threading;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class NullReporter : IBatchReporter
	{
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

		private Guid _guid = Guid.NewGuid();

		private int _rowErrors;
		private Job _job;
		private readonly IAPILog _logger;
		private readonly IErrorFilesSizeStatistics _errorFilesSizeStatistics;
		private readonly IImageFileSizeStatistics _imageFileSizeStatistics;
		private readonly IJobHistoryService _service;
		private readonly INativeFileSizeStatistics _nativeFileSizeStatistics;
		private readonly IWorkspaceDBContext _context;
		private readonly JobStatisticsQuery _query;
		private readonly TaskParameterHelper _helper;
		public IHelper Helper { get; }

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
			IErrorFilesSizeStatistics errorFilesSizeStatistics)
		{
			Helper = helper;
			_query = query;
			_helper = taskParameterHelper;
			_service = service;
			_context = context;
			_nativeFileSizeStatistics = nativeFileSizeStatistics;
			_imageFileSizeStatistics = imageFileSizeStatistics;
			_errorFilesSizeStatistics = errorFilesSizeStatistics;
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
			reporter.OnBatchComplete += OnJobComplete;
			reporter.OnDocumentError += RowError;
		}

		public void RowError(string documentIdentifier, string errorMessage)
		{
			_rowErrors++;
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
			_logger.LogWarning("[{guid}, {threadId}] Start updating {identifier}, {transferredIdem}, {errorCount}", _guid, Thread.CurrentThread.ManagedThreadId, identifier, transferredItem, erroredCount);
			try
			{
				_context.BeginTransaction();
				_logger.LogWarning("[{guid} {threadId}] After transaction", _guid, Thread.CurrentThread.ManagedThreadId);
				EnableMutex(identifier);
				_logger.LogWarning("[{guid} {threadId}] After mutex", _guid, Thread.CurrentThread.ManagedThreadId);


				Data.JobHistory historyRdo = _service.GetRdo(_helper.GetBatchInstance(_job));
				_logger.LogWarning("[{guid} {threadId}] After JobHistory.GetRdo {isRdoNull}", _guid, Thread.CurrentThread.ManagedThreadId, historyRdo == null);

				if (transferredItem > 0)
				{
					historyRdo.ItemsTransferred += transferredItem;
				}

				historyRdo.ItemsWithErrors += erroredCount;
				_service.UpdateRdo(historyRdo);
				_logger.LogWarning("[{guid} {threadId}] After Rdo Update", _guid, Thread.CurrentThread.ManagedThreadId);

				_context.CommitTransaction();
				_logger.LogWarning("[{guid} {threadId}] After transaction commit", _guid, Thread.CurrentThread.ManagedThreadId);

			}
			catch (Exception e)
			{
				_logger.LogWarning("[{guid} {threadId}] Exception: {message}", _guid, Thread.CurrentThread.ManagedThreadId, e.Message);

				LogUpdatingStatisticsError(e);
				// The mutex will be removed when the transaction is finalized
				try
				{
					_context.RollbackTransaction();
				}
				catch (Exception ex)
				{
					_logger.LogWarning("[{guid} {threadId}] Rollback error: {message}.", _guid, Thread.CurrentThread.ManagedThreadId, ex.Message);

					throw;
				}
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
					(int) _job.JobId);
			long copiedFilesFileSize = filesSize - errorsFileSize;

			return copiedFilesFileSize;
		}

		#region Logging

		private void LogUpdatingStatisticsError(Exception e)
		{
			_logger.LogError(e, "Error occurred during Job Statistics update.");
		}

		#endregion
	}
}