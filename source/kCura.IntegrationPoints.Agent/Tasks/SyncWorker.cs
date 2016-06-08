using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Authentication;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker : IntegrationPointTaskBase, ITask
	{
		private JobStatisticsService _statisticsService;
		private IEnumerable<Core.IBatchStatus> _batchStatus;

		public IEnumerable<Core.IBatchStatus> BatchStatus
		{
			get { return _batchStatus ?? (_batchStatus = new List<IBatchStatus>()); }
			set { _batchStatus = value; }
		}

		public SyncWorker(
		  ICaseServiceContext caseServiceContext,
		  IHelper helper,
		  IDataProviderFactory dataProviderFactory,
		  kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
		  kCura.IntegrationPoints.Contracts.ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
          IJobHistoryService jobHistoryService,
		  JobHistoryErrorService jobHistoryErrorService,
		  IJobManager jobManager,
		  IEnumerable<IBatchStatus> statuses,
		  JobStatisticsService statisticsService) : base(caseServiceContext,
		   helper,
		   dataProviderFactory,
		   serializer,
		   appDomainRdoSynchronizerFactoryFactory,
		   jobHistoryService,
		   jobHistoryErrorService,
		   jobManager)
		{
			BatchStatus = statuses;
			_statisticsService = statisticsService;
		}

		public void Execute(Job job)
		{
			foreach (var batchComplete in BatchStatus)
			{
				batchComplete.JobStarted(job);
			}
			ExecuteTask(job);
		}

		internal virtual void ExecuteTask(Job job)
		{
			try
			{
				kCura.Method.Injection.InjectionManager.Instance.Evaluate("640E9695-AB99-4763-ADC5-03E1252277F7");

				SetIntegrationPoint(job);

				List<string> entryIDs = GetEntryIDs(job);

				SetJobHistory();

				kCura.Method.Injection.InjectionManager.Instance.Evaluate("CB070ADB-8912-4B61-99B0-3321C0670FC6");

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
			catch (AuthenticationException e)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, e.Message, e.StackTrace);
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				//rdo last run and next scheduled time will be updated in Manager job
				_jobHistoryErrorService.CommitErrors();
				PostExecute(job);
			}
		}

		internal void PostExecute(Job job)
		{
			try
			{
				TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
				var batchInstance = taskParameters.BatchInstance;
				bool isJobComplete = _jobManager.CheckBatchJobComplete(job, batchInstance.ToString());
				if (isJobComplete)
				{
					foreach (var completedItem in BatchStatus)
					{
						try
						{
							completedItem.JobComplete(job);
						}
						catch (Exception e)
						{
							_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
						}
					}
				}
			}
			catch (Exception e)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
			}
		}

		internal virtual List<string> GetEntryIDs(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
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
			_statisticsService.Subscribe(synchronizer as IBatchReporter, job);
			_jobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		internal virtual void ExecuteImport(IEnumerable<FieldMap> fieldMap,
		  string sourceConfiguration, string destinationConfiguration, List<string> entryIDs,
		  Data.SourceProvider sourceProviderRdo, Data.DestinationProvider destinationProvider, Job job)
		{
			FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();

			IDataSourceProvider sourceProvider = GetSourceProvider(SourceProvider, job);

			List<FieldEntry> sourceFields = GetSourceFields(fieldMaps);

			IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, sourceConfiguration);

			IDataSynchronizer dataSynchronizer = GetDestinationProvider(destinationProvider, destinationConfiguration, job);

			SetupSubscriptions(dataSynchronizer, job);

			if (SourceProvider.Config.GetDataProvideAllFieldsRequired)
			{
				dataSynchronizer.SyncData(sourceDataReader, fieldMaps, destinationConfiguration);
			}
			else
			{
				IEnumerable<IDictionary<FieldEntry, object>> sourceData = GetSourceData(sourceFields, sourceDataReader);
				dataSynchronizer.SyncData(sourceData, fieldMaps, destinationConfiguration);
			}
		}

		private void InjectErrors()
		{
			try
			{
				kCura.Method.Injection.InjectionManager.Instance.Evaluate("DFE4D63C-3A6A-49C2-A80D-25CA60F2B31C");
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}

			try
			{
				kCura.Method.Injection.InjectionManager.Instance.Evaluate("40af620b-af2e-4b50-9f62-870654819df6");
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyUniqueIdentifier", ex.Message, ex.StackTrace);
			}
		}
	}
}