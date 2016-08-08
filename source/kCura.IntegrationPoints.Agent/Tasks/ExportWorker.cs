using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportWorker : SyncWorker
	{
		#region Fields

		private readonly ExportProcessRunner _exportProcessRunner;

		#endregion //Fields

		#region Constructor

		public ExportWorker(ICaseServiceContext caseServiceContext, IHelper helper,
			IDataProviderFactory dataProviderFactory, ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory, IJobHistoryService jobHistoryService,
			JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider, IJobManager jobManager, IEnumerable<IBatchStatus> statuses,
			JobStatisticsService statisticsService, ExportProcessRunner exportProcessRunner, IManagerFactory managerFactory,
			IRepositoryFactory repositoryFactory, IContextContainerFactory contextContainerFactory, IJobService jobService)
			: base(
				caseServiceContext, helper, dataProviderFactory, serializer, appDomainRdoSynchronizerFactoryFactory,
				jobHistoryService, jobHistoryErrorServiceProvider.JobHistoryErrorService, jobManager, statuses, statisticsService, managerFactory, repositoryFactory, contextContainerFactory, jobService)
		{
			_exportProcessRunner = exportProcessRunner;
		}

		#endregion //Constructor

		#region Methods

		protected override IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo,
			string configuration, Job job)
		{
			var providerGuid = new Guid(destinationProviderRdo.Identifier);

			var sourceProvider = AppDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}

		internal override void ExecuteImport(IEnumerable<FieldMap> fieldMap, string sourceConfiguration,
			string destinationConfiguration, List<string> entryIDs,
			SourceProvider sourceProviderRdo, DestinationProvider destinationProvider, Job job)
		{
			var sourceSettings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(sourceConfiguration);

			var destinationSettings = JsonConvert.DeserializeObject<ImportSettings>(destinationConfiguration);

			_exportProcessRunner.StartWith(sourceSettings, fieldMap, destinationSettings.ArtifactTypeId);
		}

		#endregion //Methods
	}
}