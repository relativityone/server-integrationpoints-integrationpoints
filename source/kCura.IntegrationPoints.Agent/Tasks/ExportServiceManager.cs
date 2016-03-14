using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportServiceManager : ITask
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly JobHistoryService _jobHistoryService;
		private readonly JobHistoryErrorService _jobHistoryErrorService;
		private readonly ISynchronizerFactory _synchronizerFactory;
		private readonly JobStatisticsService _statisticsService;
		private readonly IEnumerable<IBatchStatus> _batchStatus;
		private readonly Apps.Common.Utils.Serializers.ISerializer _serializer;
		private Guid _identifier;

		public ExportServiceManager(
			ICaseServiceContext caseServiceContext,
			ISynchronizerFactory synchronizerFactory,
			IEnumerable<IBatchStatus> statuses,
			kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
			JobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			JobStatisticsService statisticsService)
		{
			_synchronizerFactory = synchronizerFactory;
			_caseServiceContext = caseServiceContext;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_statisticsService = statisticsService;
			_batchStatus = statuses ?? new List<IBatchStatus>();
			_serializer = serializer;
		}

		public IntegrationPoint IntegrationPointDto { get; private set; }
		public JobHistory JobHistoryDto { get; private set; }
		public List<FieldMap> MappedFields { get; private set; }
		public SourceProvider SourceProvider { get; private set; }

		public void Execute(Job job)
		{
			try
			{
				InitializeExportService(job);

				string destinationConfig = IntegrationPointDto.DestinationConfiguration;
				DirectSqlCallHelper helper = new DirectSqlCallHelper(_caseServiceContext.SqlContext);

				IDataSynchronizer synchronizer = GetRdoDestinationProviderAsync(destinationConfig);

				SetupSubscriptions(synchronizer, job);

				// Initialize Exporter
				IExporterService exporter = ExporterFactory.BuildExporter(helper, MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration);
				JobHistoryDto.TotalItems = exporter.TotalRecordsFound;
				UpdateJobStatus();

				IEnumerable<FieldEntry> sources = MappedFields.Select(map => map.SourceField);
				synchronizer.SyncData(new DocumentTransferDataReader(exporter, sources, helper), MappedFields, destinationConfig);
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
				PostExecute(job);
			}
		}

		protected void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			_statisticsService.Subscribe(synchronizer as IBatchReporter, job);
			_jobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		private void InitializeExportService(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			this._identifier = taskParameters.BatchInstance;

			// Load integrationPoint data
			if (IntegrationPointDto != null) return;

			int integrationPointId = job.RelatedObjectArtifactID;
			this.IntegrationPointDto = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointId);

			if (this.IntegrationPointDto == null)
			{
				throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
			}

			this.JobHistoryDto = _jobHistoryService.GetRdo(this._identifier);
			_jobHistoryErrorService.JobHistory = this.JobHistoryDto;
			_jobHistoryErrorService.IntegrationPoint = this.IntegrationPointDto;

			// Load Mapped Fields & Sanatized Them
			// #unbelievable
			MappedFields = JsonConvert.DeserializeObject<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			// Load Source Provider, which I think should be named something else.
			SourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(IntegrationPointDto.SourceProvider.Value);

			this.JobHistoryDto.StartTimeUTC = DateTime.UtcNow;
			UpdateJobStatus();
			foreach (var batchComplete in _batchStatus)
			{
				batchComplete.JobStarted(job);
			}
		}

		internal void PostExecute(Job job)
		{
			try
			{
				foreach (var completedItem in _batchStatus)
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
			catch (Exception e)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
			}
		}

		internal IDataSynchronizer GetRdoDestinationProviderAsync(string configuration)
		{
			// RDO synchronizer
			Guid providerGuid = new Guid("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
			var factory = _synchronizerFactory as GeneralWithCustodianRdoSynchronizerFactory;
			if (factory != null)
			{
				factory.SourceProvider = SourceProvider;
			}
			IDataSynchronizer synchronizer = _synchronizerFactory.CreateSynchronizer(providerGuid, configuration);
			return synchronizer;	
		}

		private void UpdateJobStatus()
		{
			_jobHistoryService.UpdateRdo(JobHistoryDto);
		}
	}
}