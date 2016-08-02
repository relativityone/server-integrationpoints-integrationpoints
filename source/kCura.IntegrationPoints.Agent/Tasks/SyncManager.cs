using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.ScheduleQueue.Core.BatchProcess;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	using global::kCura.Method.Injection;

	using Config = global::kCura.IntegrationPoints.Config.Config;

	public class SyncManager : BatchManagerBase<string>, IDisposable
	{
		private ICaseServiceContext _caseServiceContext;
		private IDataProviderFactory _providerFactory;
		private IJobManager _jobManager;
		private IJobService _jobService;
		private readonly IHelper _helper;
		private IIntegrationPointService _integrationPointService;
		private IScheduleRuleFactory _scheduleRuleFactory;
		private ISerializer _serializer;
		private IGuidService _guidService;
		private IJobHistoryService _jobHistoryService;
		private JobHistoryErrorService _jobHistoryErrorService;
		private IEnumerable<Core.IBatchStatus> _batchStatus;
		private bool _errorOccurred;

		public IEnumerable<Core.IBatchStatus> BatchStatus
		{
			get { return _batchStatus ?? (_batchStatus = new List<IBatchStatus>()); }
			set { _batchStatus = value; }
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
			JobHistoryErrorService jobHistoryErrorService,
			IScheduleRuleFactory scheduleRuleFactory,
			IEnumerable<IBatchStatus> batchStatuses)
		{
			_caseServiceContext = caseServiceContext;
			_providerFactory = providerFactory;
			_jobManager = jobManager;
			_jobService = jobService;
			_helper = helper;
			_integrationPointService = integrationPointService;
			_serializer = serializer;
			_guidService = guidService;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_scheduleRuleFactory = scheduleRuleFactory;
			base.RaiseJobPreExecute += new JobPreExecuteEvent(JobPreExecute);
			base.RaiseJobPostExecute += new JobPostExecuteEvent(JobPostExecute);
			BatchJobCount = 0;
			BatchInstance = Guid.NewGuid();
			_batchStatus = batchStatuses;
			_errorOccurred = false;
		}

		public Data.IntegrationPoint IntegrationPoint { get; set; }
		public Data.JobHistory JobHistory { get; set; }
		public Guid BatchInstance { get; set; }
		public int BatchJobCount { get; set; }

		public override int BatchSize
		{
			get { return Config.Instance.BatchSize; }
		}

		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			try
			{
				if (string.IsNullOrEmpty(job.JobDetails))
				{
					//job is scheduled so give it the same look as import now
					var details = new TaskParameters()
					{
						BatchInstance = this.BatchInstance
					};
					job.JobDetails = _serializer.Serialize(details);
				}
				foreach (var batchStatus in BatchStatus)
				{
					batchStatus.OnJobStart(job);
				}
				SourceProvider sourceProviderRdo = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(this.IntegrationPoint.SourceProvider.Value);
				Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
				Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
				IDataSourceProvider provider = _providerFactory.GetDataProvider(applicationGuid, providerGuid, _helper);
				FieldEntry idField = _integrationPointService.GetIdentifierFieldEntry(this.IntegrationPoint.ArtifactId);
				string options = _integrationPointService.GetSourceOptions(this.IntegrationPoint.ArtifactId);
				IDataReader idReader = provider.GetBatchableIds(idField, options);

				return new ReaderEnumerable(idReader);
			}
			catch (Exception ex)
			{
				_errorOccurred = true;
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
			TaskParameters taskParameters = new TaskParameters()
			{
				BatchInstance = this.BatchInstance,
				BatchParameters = batchIDs
			};
			_jobManager.CreateJobWithTracker(job, taskParameters, GetTaskType(), this.BatchInstance.ToString());
			BatchJobCount++;
		}
		
	    protected virtual TaskType GetTaskType()
	    {
	        return TaskType.SyncWorker;
	    }

        private class ReaderEnumerable : IEnumerable<string>
		{
			private IDataReader _reader;
			public ReaderEnumerable(IDataReader reader)
			{
				_reader = reader;
			}
			public IEnumerator<string> GetEnumerator()
			{
				while (_reader.Read())
				{
					var result = _reader.GetString(0);
					yield return result;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private void JobPreExecute(Job job)
		{
			try
			{
				InjectionManager.Instance.Evaluate("B50CD1DD-6FEC-439E-A730-B84B730C9D44");

				this.BatchInstance = GetBatchInstance(job);
				if (job.RelatedObjectArtifactID < 1)
				{
					throw new ArgumentNullException("Job must have a Related Object ArtifactID");
				}

				this.IntegrationPoint = _integrationPointService.GetRdo(job.RelatedObjectArtifactID);
				if (this.IntegrationPoint.SourceProvider == 0)
				{
					throw new Exception("Cannot import source provider with unknown id.");
				}
				this.JobHistory = _jobHistoryService.CreateRdo(this.IntegrationPoint, this.BatchInstance, DateTime.UtcNow);
				_jobHistoryErrorService.JobHistory = this.JobHistory;
				_jobHistoryErrorService.IntegrationPoint = IntegrationPoint;
				InjectionManager.Instance.Evaluate("0F8D9778-5228-4D7A-A911-F731292F9CF0");

				if (!this.JobHistory.StartTimeUTC.HasValue)
				{
					this.JobHistory.StartTimeUTC = DateTime.UtcNow;
					//TODO: jobHistory.Status = "";
					_jobHistoryService.UpdateRdo(this.JobHistory);
				}
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
				this.IntegrationPoint.LastRuntimeUTC = DateTime.UtcNow;
				if (job.SerializedScheduleRule != null)
				{
					this.IntegrationPoint.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory,
						taskResult);
				}
				_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(this.IntegrationPoint);

				if (this.JobHistory != null)
				{
					this.JobHistory.TotalItems = items;
					if (BatchJobCount == 0)
					{
						//no worker jobs were submitted
						this.JobHistory.EndTimeUTC = DateTime.UtcNow;
						if (_errorOccurred)
						{
							this.JobHistory.JobStatus = JobStatusChoices.JobHistoryErrorJobFailed;
							_errorOccurred = false;
						}
						else
						{
							this.JobHistory.JobStatus = JobStatusChoices.JobHistoryCompleted;
						}
					}
					_caseServiceContext.RsapiService.JobHistoryLibrary.Update(this.JobHistory);
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

		public Guid GetBatchInstance(Job job)
		{
			return new TaskParameterHelper(_serializer, _guidService).GetBatchInstance(job);
		}

		public void Dispose()
		{
			Debug.WriteLine("");
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
