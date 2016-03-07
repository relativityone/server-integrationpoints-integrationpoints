using System;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class NullReporter : IBatchReporter
	{
		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit;
		public event BatchCreated OnBatchCreate;
		public event StatusUpdate OnStatusUpdate;
		public event JobError OnJobError;
		public event RowError OnDocumentError;
	}

	public class JobStatisticsService
	{
		private readonly JobStatisticsQuery _query;
		private readonly TaskParameterHelper _helper;
		private readonly JobHistoryService _service;
		private IWorkspaceDBContext _context;
		private Job _job;

		private int _rowErrors = 0;

		public JobStatisticsService(JobStatisticsQuery query,
			TaskParameterHelper helper,
			JobHistoryService service,
			IWorkspaceDBContext context)
		{
			_query = query;
			_helper = helper;
			_service = service;
			_context = context;
		}

		public void Subscribe(IBatchReporter reporter, Job job)
		{
			if (reporter == null)
			{
				reporter = new NullReporter();
			}
			_job = job;
			reporter.OnStatusUpdate += this.StatusUpdate;
			reporter.OnBatchComplete += this.JobComplete;
			reporter.OnDocumentError += RowError;
		}

		public void RowError(string documentIdentifier, string errorMessage)
		{
			_rowErrors++;
		}

		private void JobComplete(DateTime start, DateTime end, int total, int errorCount)
		{
			//skip errorCount because we do suppress some errors so RowError is a more reliable mechanism 
			string tableName = JobTracker.GenerateJobTrackerTempTableName(_job, _helper.GetBatchInstance(_job).ToString());
			JobStatistics stats = _query.UpdateAndRetrieveStats(tableName, _job.JobId, new JobStatistics { Completed = total, Errored = _rowErrors });
			_rowErrors = 0;
			Data.JobHistory historyRdo = _service.GetRdo(_helper.GetBatchInstance(_job));
			historyRdo.RecordsImported = stats.Imported;
			historyRdo.RecordsWithErrors = stats.Errored;
			_service.UpdateRdo(historyRdo);
		}

		private void StatusUpdate(int count)
		{
			Update(_helper.GetBatchInstance(_job), count);
		}

		/// <summary>
		/// Multiple agents may be trying to access Job History data at the same time. Since agents can run across
		/// machines our database is used to act as a distributed lock coordinator. The mutex is managed by
		/// sp_getapplock and sp_releaseapplock.
		/// This prevents read -> read -> update -> update 
		///	when we require read -> update -> read -> update.
		/// 
		/// For more information see:
		/// sp_getapplock - https://msdn.microsoft.com/en-us/library/ms189823.aspx
		/// sp_releaseapplock - https://msdn.microsoft.com/en-us/library/ms178602.aspx
		/// </summary>
		public void Update(Guid identifier, int count)
		{
			try
			{
				_context.BeginTransaction();
				EnableMutex(identifier);

				Data.JobHistory historyRdo = _service.GetRdo(_helper.GetBatchInstance(_job));
				historyRdo.RecordsImported += count;
				_service.UpdateRdo(historyRdo);

				_context.CommitTransaction();
			}
			catch
			{
				// The mutex will be removed when the transaction is finalized
				_context.RollbackTransaction();
			}
		}

		/// <summary>
		/// This method must be called within a transaction
		/// </summary>
		private void EnableMutex(Guid identifier)
		{
			string enableJobHistoryMutex = String.Format(@"
				DECLARE @res INT
				EXEC @res = sp_getapplock
								@Resource = '{0}',
								@LockMode = 'Exclusive',
								@LockOwner = 'Transaction',
								@LockTimeout = 5000,
								@DbPrincipal = 'public'

				IF @res NOT IN (0, 1)
						BEGIN
							RAISERROR ( 'Unable to acquire mutex', 16, 1 )
						END", identifier);

			_context.ExecuteNonQuerySQLStatement(enableJobHistoryMutex);
		}
	}
}
