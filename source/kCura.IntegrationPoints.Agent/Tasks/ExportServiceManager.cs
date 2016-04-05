using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportServiceManager : ITask
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly JobHistoryService _jobHistoryService;
		private readonly JobHistoryErrorService _jobHistoryErrorService;
		private readonly ISynchronizerFactory _synchronizerFactory;
		private readonly IExporterFactory _exporterFactory;
		private readonly JobStatisticsService _statisticsService;
		private readonly IEnumerable<IBatchStatus> _batchStatus;
		private readonly Apps.Common.Utils.Serializers.ISerializer _serializer;
		private Guid _identifier;
		private int _destinationWorkspaceId;
		private readonly IHelper _helper;
		private SourceConfiguration _sourceConfiguration;

		public ExportServiceManager(
			ICaseServiceContext caseServiceContext,
			ISynchronizerFactory synchronizerFactory,
			IExporterFactory exporterFactory,
			IEnumerable<IBatchStatus> statuses,
			kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
			JobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			JobStatisticsService statisticsService,
			IHelper helper)
		{
			_synchronizerFactory = synchronizerFactory;
			_exporterFactory = exporterFactory;
			_caseServiceContext = caseServiceContext;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_statisticsService = statisticsService;
			_batchStatus = statuses ?? new List<IBatchStatus>();
			_serializer = serializer;
			_helper = helper;
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
				IDataSynchronizer synchronizer = GetRdoDestinationProvider(destinationConfig);

				SetupSubscriptions(synchronizer, job);

				// Initialize Exporter
				IExporterService exporter = _exporterFactory.BuildExporter(MappedFields.ToArray(), IntegrationPointDto.SourceConfiguration);
				JobHistoryDto.TotalItems = exporter.TotalRecordsFound;
				UpdateJobStatus();

				synchronizer.SyncData(exporter.GetDataReader(this._identifier.ToString()), MappedFields, destinationConfig);
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
				PostExecute(job);
				new DestinationWorkspaceManager(_caseServiceContext, _helper, job, _sourceConfiguration, this._identifier.ToString()).Execute();
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
			if (IntegrationPointDto != null)
			{
				return;
			}

			int integrationPointId = job.RelatedObjectArtifactID;
			this.IntegrationPointDto = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointId);

			if (this.IntegrationPointDto == null)
			{
				throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
			}

			this.JobHistoryDto = _jobHistoryService.GetRdo(this._identifier);
			_jobHistoryErrorService.JobHistory = this.JobHistoryDto;
			_jobHistoryErrorService.IntegrationPoint = this.IntegrationPointDto;
			_jobHistoryErrorService.SetTableSuffix(this._identifier.ToString());

			// Load Mapped Fields & Sanitize them
			// #unbelievable
			MappedFields = JsonConvert.DeserializeObject<List<FieldMap>>(IntegrationPointDto.FieldMappings);
			MappedFields.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);

			_sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(IntegrationPointDto.SourceConfiguration);

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
			foreach (IBatchStatus completedItem in _batchStatus)
			{
				try
				{
					completedItem.JobComplete(job);
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

			try
			{
				IntegrationPointDto.LastRuntimeUTC = DateTime.UtcNow;
				_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(this.IntegrationPointDto);
			}
			catch
			{
				// ignored
			}
		}

		internal IDataSynchronizer GetRdoDestinationProvider(string configuration)
		{
			// if you want to create add another synchronizer aka exporter, you may add it here.
			// RDO synchronizer
			Guid providerGuid = new Guid("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
			GeneralWithCustodianRdoSynchronizerFactory factory = _synchronizerFactory as GeneralWithCustodianRdoSynchronizerFactory;
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