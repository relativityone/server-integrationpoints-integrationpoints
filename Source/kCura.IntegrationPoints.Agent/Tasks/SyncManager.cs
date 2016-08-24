using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.BatchProcess;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	using Method.Injection;

	using Config = Config.Config;

	[SynchronizedTask]
	public class SyncManager : BatchManagerBase<string>
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IDataProviderFactory _providerFactory;
		private readonly IJobManager _jobManager;
		private readonly IJobService _jobService;
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IScheduleRuleFactory _scheduleRuleFactory;
		private readonly IGuidService _guidService;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IJobHistoryErrorService _jobHistoryErrorService;
		private IEnumerable<Core.IBatchStatus> _batchStatus;

		protected readonly IHelper Helper;
		protected readonly IManagerFactory ManagerFactory;
		protected readonly IContextContainerFactory ContextContainerFactory;
		protected readonly ISerializer Serializer;

		public IEnumerable<IBatchStatus> BatchStatus
		{
			get { return _batchStatus ?? (_batchStatus = new List<IBatchStatus>()); }
		}

		public SyncManager(ICaseServiceContext caseServiceContext,
			IDataProviderFactory providerFactory,
			IJobManager jobManager,
			IJobService jobService,
			IHelper helper,
			IIntegrationPointService integrationPointService,
			ISerializer serializer,
			IGuidService guidService,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IScheduleRuleFactory scheduleRuleFactory,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IEnumerable<IBatchStatus> batchStatuses)
		{
			_caseServiceContext = caseServiceContext;
			_providerFactory = providerFactory;
			_jobManager = jobManager;
			_jobService = jobService;
			Helper = helper;
			_integrationPointService = integrationPointService;
			Serializer = serializer;
			_guidService = guidService;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_scheduleRuleFactory = scheduleRuleFactory;
			ManagerFactory = managerFactory;
			ContextContainerFactory = contextContainerFactory;
			RaiseJobPreExecute += JobPreExecute;
			RaiseJobPostExecute += JobPostExecute;
			BatchJobCount = 0;
			BatchInstance = Guid.NewGuid();
			_batchStatus = batchStatuses;
		}

		public IntegrationPoint IntegrationPoint { get; set; }
		public JobHistory JobHistory { get; set; }
		public IJobStopManager JobStopManager { get; set; }
		public Guid BatchInstance { get; set; }
		public int BatchJobCount { get; set; }

		public override int BatchSize => Config.Instance.BatchSize;

		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			try
			{
				if (string.IsNullOrEmpty(job.JobDetails))
				{
					//job is scheduled so give it the same look as import now
					var details = new TaskParameters()
					{
						BatchInstance = BatchInstance
					};
					job.JobDetails = Serializer.Serialize(details);
				}
				foreach (var batchStatus in BatchStatus)
				{
					batchStatus.OnJobStart(job);
				}

				JobStopManager?.ThrowIfStopRequested();

				SourceProvider sourceProviderRdo = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPoint.SourceProvider.Value);
				Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
				Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
				IDataSourceProvider provider = _providerFactory.GetDataProvider(applicationGuid, providerGuid, Helper);

				JobStopManager?.ThrowIfStopRequested();

				FieldEntry idField = _integrationPointService.GetIdentifierFieldEntry(IntegrationPoint.ArtifactId);
				string options = _integrationPointService.GetSourceOptions(IntegrationPoint.ArtifactId);
				IDataReader idReader = provider.GetBatchableIds(idField, options);

				JobStopManager?.ThrowIfStopRequested();

				return new ReaderEnumerable(idReader, JobStopManager);
			}
			catch (OperationCanceledException)
			{
				JobStopManager?.Dispose();
				throw;
				// DO NOTHING. Someone attempted to stop the job.
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
			}
			return new List<string>();
		}

		public override void CreateBatchJob(Job job, List<string> batchIDs)
		{
			JobStopManager?.ThrowIfStopRequested();

			TaskParameters taskParameters = new TaskParameters()
			{
				BatchInstance = BatchInstance,
				BatchParameters = batchIDs
			};
			_jobManager.CreateJobWithTracker(job, taskParameters, GetTaskType(), BatchInstance.ToString());
			BatchJobCount++;
		}

		protected virtual TaskType GetTaskType()
		{
			return TaskType.SyncWorker;
		}

		private class ReaderEnumerable : IEnumerable<string>, IDisposable
		{
			private readonly IDataReader _reader;
			private readonly IJobStopManager _jobStopManager;

			public ReaderEnumerable(IDataReader reader, IJobStopManager jobStopManager)
			{
				_reader = reader;
				_jobStopManager = jobStopManager;
			}

			public IEnumerator<string> GetEnumerator()
			{
				while (_reader.Read())
				{
					_jobStopManager?.ThrowIfStopRequested();

					var result = _reader.GetString(0);
					yield return result;
				}
				Dispose();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			private void Dispose(bool disposing)
			{
				if (disposing)
				{
					_jobStopManager?.Dispose();
					_reader?.Dispose();
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		private void JobPreExecute(Job job)
		{
			try
			{
				InjectionManager.Instance.Evaluate("B50CD1DD-6FEC-439E-A730-B84B730C9D44");

				BatchInstance = GetBatchInstance(job);
				if (job.RelatedObjectArtifactID < 1)
				{
					throw new ArgumentNullException("Job must have a Related Object ArtifactID");
				}

				IntegrationPoint = _integrationPointService.GetRdo(job.RelatedObjectArtifactID);
				if (IntegrationPoint.SourceProvider == 0)
				{
					throw new Exception("Cannot import source provider with unknown id.");
				}
				this.JobHistory = _jobHistoryService.GetOrCreateSchduledRunHistoryRdo(this.IntegrationPoint, this.BatchInstance, DateTime.UtcNow);
				_jobHistoryErrorService.JobHistory = this.JobHistory;
				_jobHistoryErrorService.IntegrationPoint = IntegrationPoint;
				InjectionManager.Instance.Evaluate("0F8D9778-5228-4D7A-A911-F731292F9CF0");

				JobStopManager = ManagerFactory.CreateJobStopManager(_jobService, _jobHistoryService, BatchInstance, job.JobId, true);
				JobStopManager.ThrowIfStopRequested();

				if (!JobHistory.StartTimeUTC.HasValue)
				{
					JobHistory.StartTimeUTC = DateTime.UtcNow;
					//TODO: jobHistory.Status = "";
					_jobHistoryService.UpdateRdo(JobHistory);
				}
			}
			catch (OperationCanceledException)
			{
				JobStopManager.Dispose();
				throw;
				// DO NOTHING. Someone attempted to stop the job.
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
			}
		}

		private void JobPostExecute(Job job, TaskResult taskResult, int items)
		{
			try
			{
				List<Exception> exceptions = new List<Exception>();
				try
				{
					UpdateLastRuntimeAndCalculateNextRuntime(job, taskResult);
				}
				catch (Exception exception)
				{
					exceptions.Add(exception);
				}

				if (this.JobHistory != null)
				{
					if (BatchJobCount == 0)
					{
						try
						{
							FinalizeJob(job);
						}
						catch (Exception exception)
						{
							exceptions.Add(exception);
						}
					}

					try
					{
						JobHistory.TotalItems = items;
						_caseServiceContext.RsapiService.JobHistoryLibrary.Update(JobHistory);
					}
					catch (Exception exception)
					{
						exceptions.Add(exception);
					}
				}

				if (exceptions.Any())
				{
					throw new AggregateException(exceptions);
				}
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, new Exception("Failed to update job statistics.", ex));
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
			}
		}

		private void UpdateStopState(Job job)
		{
			if (job.SerializedScheduleRule != null)
			{
				_jobService.UpdateStopState(new List<long>() { job.JobId }, StopState.None);
			}
		}

		private void FinalizeJob(Job job)
		{
			List<Exception> exceptions = new List<Exception>();

			try
			{
				UpdateStopState(job);
			}
			catch (Exception exception)
			{
				exceptions.Add(exception);
			}

			foreach (var batchStatus in BatchStatus)
			{
				try
				{
					batchStatus.OnJobComplete(job);
				}
				catch (Exception exception)
				{
					exceptions.Add(exception);
				}
			}

			try
			{
				if (JobHistory != null && JobStopManager.IsStopRequested())
				{
					IContextContainer contextContainer = ContextContainerFactory.CreateContextContainer(Helper);
					IJobHistoryManager jobHistoryManager = ManagerFactory.CreateJobHistoryManager(contextContainer);
					jobHistoryManager.SetErrorStatusesToExpired(_caseServiceContext.WorkspaceID, JobHistory.ArtifactId);
				}
			}
			catch (Exception exception)
			{
				exceptions.Add(exception);
			}

			if (exceptions.Any())
			{
				throw new AggregateException("Failed to finalize the job.", exceptions);
			}
		}

		private void UpdateLastRuntimeAndCalculateNextRuntime(Job job, TaskResult taskResult)
		{
			IntegrationPoint.LastRuntimeUTC = DateTime.UtcNow;
			if (job.SerializedScheduleRule != null)
			{
				IntegrationPoint.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory, taskResult);
			}
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(IntegrationPoint);
		}

		public Guid GetBatchInstance(Job job)
		{
			return new TaskParameterHelper(Serializer, _guidService).GetBatchInstance(job);
		}

		public override void Execute(Job job)
		{
			using (Client.MetricsClient.LogDuration(
					Core.Constants.IntegrationPoints.Telemetry.BUCKET_SYNC_MANAGER_EXEC_DURATION_METRIC_COLLECTOR,
					Guid.Empty, MetricTargets.APMandSUM))
			{
				base.Execute(job);
			}
		}
	}
}