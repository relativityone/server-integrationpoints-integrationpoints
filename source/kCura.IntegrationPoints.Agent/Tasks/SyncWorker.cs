using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Method.Injection;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Authentication;
using System;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker : IntegrationPointTaskBase, ITask
	{
		private readonly JobStatisticsService _statisticsService;
		private readonly IRepositoryFactory _repositoryFactory;
		private IEnumerable<IBatchStatus> _batchStatus;

		public IEnumerable<IBatchStatus> BatchStatus
		{
			get { return _batchStatus ?? (_batchStatus = new List<IBatchStatus>()); }
			set { _batchStatus = value; }
		}

		public SyncWorker(
		  ICaseServiceContext caseServiceContext,
		  IHelper helper,
		  IDataProviderFactory dataProviderFactory,
		  ISerializer serializer,
		  ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
		  IJobHistoryService jobHistoryService,
		  JobHistoryErrorService jobHistoryErrorService,
		  IJobManager jobManager,
		  IEnumerable<IBatchStatus> statuses,
		  JobStatisticsService statisticsService,
		  IManagerFactory managerFactory,
		  IRepositoryFactory repositoryFactory,
		  IContextContainerFactory contextContainerFactory,
		  IJobService jobService) :
			base(caseServiceContext,
				helper,
				dataProviderFactory,
				serializer,
				appDomainRdoSynchronizerFactoryFactory,
				jobHistoryService,
				jobHistoryErrorService,
				jobManager,
				managerFactory,
				contextContainerFactory,
				jobService)
		{
			BatchStatus = statuses;
			_statisticsService = statisticsService;
			_repositoryFactory = repositoryFactory;
		}

		public void Execute(Job job)
		{
			using (Client.MetricsClient.LogDuration(
				Core.Constants.IntegrationPoints.Telemetry.BUCKET_SYNC_WORKER_EXEC_DURATION_METRIC_COLLECTOR, Guid.Empty, MetricTargets.APMandSUM))
			{
				foreach (var batchComplete in BatchStatus)
				{
					batchComplete.OnJobStart(job);
				}
				ExecuteTask(job);
			}
		}

		protected virtual void ExecuteTask(Job job)
		{
			try
			{
				InjectionManager.Instance.Evaluate("640E9695-AB99-4763-ADC5-03E1252277F7");

				SetIntegrationPoint(job);

				List<string> entryIDs = GetEntryIDs(job);

				SetJobHistory();

				JobHistoryErrorService.JobStopManager = GetJobStopManager(job);

				InjectionManager.Instance.Evaluate("CB070ADB-8912-4B61-99B0-3321C0670FC6");

				if (!this.IntegrationPoint.SourceProvider.HasValue)
				{
					throw new ArgumentException("Cannot import source provider with unknown id.");
				}
				if (!this.IntegrationPoint.DestinationProvider.HasValue)
				{
					throw new ArgumentException("Cannot import destination provider with unknown id.");
				}
				IEnumerable<FieldMap> fieldMap = GetFieldMap(this.IntegrationPoint.FieldMappings);
				string sourceConfiguration = GetSourceConfiguration(this.IntegrationPoint.SourceConfiguration);

				ExecuteImport(fieldMap, sourceConfiguration, this.IntegrationPoint.DestinationConfiguration, entryIDs, SourceProvider, DestinationProvider, job);

				InjectErrors();
			}
			catch (OperationCanceledException)
			{
				IJobStopManager jobStopManaer = this.GetJobStopManager(job);
				jobStopManaer.Dispose();
				// IGNORE ERROR. The user attempted to stop the job.
			}
			catch (AuthenticationException e)
			{
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, e.Message, e.StackTrace);
			}
			catch (Exception ex)
			{
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				//rdo last run and next scheduled time will be updated in Manager job
				JobHistoryErrorService.CommitErrors();
				PostExecute(job);
			}
		}

		protected void PostExecute(Job job)
		{
			try
			{
				TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
				var batchInstance = taskParameters.BatchInstance;
				bool isJobComplete = JobManager.CheckBatchOnJobComplete(job, batchInstance.ToString());

				IJobStopManager jobStopManaer = this.GetJobStopManager(job);
				jobStopManaer.Dispose();

				if (isJobComplete)
				{
					try
					{
						if (!GetJobStopManager(job).IsStopRequested())
						{
							IList<Job> jobs = JobManager.GetJobsByBatchInstanceId(IntegrationPoint.ArtifactId, taskParameters.BatchInstance);
							if (jobs.Any())
							{
								JobService.UpdateStopState(jobs.Select(agentJob => agentJob.JobId).ToList(), StopState.Unstoppable);
							}
						}
					}
					catch
					{
						// IGNORE ERROR. It is possible that the user stop the job in between disposing job history manager and the updating the stop state.
					}

					SetErrorStatusesToExpiredIfStopped(job, _repositoryFactory);

					foreach (var completedItem in BatchStatus)
					{
						try
						{
							completedItem.OnJobComplete(job);
						}
						catch (Exception e)
						{
							JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
						}
					}

					IList<Job> jobsToUpdate = JobManager.GetJobsByBatchInstanceId(IntegrationPoint.ArtifactId, taskParameters.BatchInstance);
					if (jobsToUpdate.Any())
					{
						JobService.UpdateStopState(jobsToUpdate.Select(agentJob => agentJob.JobId).ToList(), StopState.None);
					}
				}
			}
			catch (Exception e)
			{
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
			}
			finally
			{
				JobHistoryErrorService.CommitErrors();
			}
		}

		protected virtual List<string> GetEntryIDs(Job job)
		{
			TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			this.BatchInstance = taskParameters.BatchInstance;
			if (taskParameters.BatchParameters != null)
			{
				if (taskParameters.BatchParameters is Newtonsoft.Json.Linq.JArray)
				{
					return ((Newtonsoft.Json.Linq.JArray)taskParameters.BatchParameters).ToObject<List<string>>();
				}
				else if (taskParameters.BatchParameters is List<string>)
				{
					return (List<string>)taskParameters.BatchParameters;
				}
			}
			return new List<string>();
		}

		protected void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			SetupStatisticsSubscriptions(synchronizer, job);
			SetupJobHistoryErrorSubscriptions(synchronizer, job);
		}

		protected void SetupStatisticsSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			_statisticsService.Subscribe(synchronizer as IBatchReporter, job);
		}

		protected void SetupJobHistoryErrorSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			JobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		protected virtual void ExecuteImport(IEnumerable<FieldMap> fieldMap,
		  string sourceConfiguration, string destinationConfiguration, List<string> entryIDs,
		  SourceProvider sourceProviderRdo, Data.DestinationProvider destinationProvider, Job job)
		{
			FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();

			IDataSourceProvider sourceProvider = GetSourceProvider(SourceProvider, job);

			this.ThrowIfStopRequested(job);

			List<FieldEntry> sourceFields = GetSourceFields(fieldMaps);

			IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, sourceConfiguration);
			IDataSynchronizer dataSynchronizer = GetDestinationProvider(destinationProvider, destinationConfiguration, job);

			if (dataSynchronizer is RdoSynchronizerBase)
			{
				ImportSettings settings = Serializer.Deserialize<ImportSettings>(destinationConfiguration);
				settings.OnBehalfOfUserId = job.SubmittedBy;
				destinationConfiguration = Serializer.Serialize(settings);
			}

			SetupSubscriptions(dataSynchronizer, job);

			IEnumerable<IDictionary<FieldEntry, object>> sourceData = GetSourceData(sourceFields, sourceDataReader);

			this.ThrowIfStopRequested(job);

			dataSynchronizer.SyncData(sourceData, fieldMaps, destinationConfiguration);
		}

		private void InjectErrors()
		{
			try
			{
				InjectionManager.Instance.Evaluate("DFE4D63C-3A6A-49C2-A80D-25CA60F2B31C");
			}
			catch (Exception ex)
			{
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}

			try
			{
				InjectionManager.Instance.Evaluate("40af620b-af2e-4b50-9f62-870654819df6");
			}
			catch (Exception ex)
			{
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyUniqueIdentifier", ex.Message, ex.StackTrace);
			}
		}
	}
}