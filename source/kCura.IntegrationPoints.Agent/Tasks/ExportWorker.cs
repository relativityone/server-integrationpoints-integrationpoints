using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportWorker : SyncWorker
	{
		public ExportWorker(ICaseServiceContext caseServiceContext, IHelper helper, IDataProviderFactory dataProviderFactory, ISerializer serializer, ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory, IJobHistoryService jobHistoryService, JobHistoryErrorService jobHistoryErrorService, IJobManager jobManager, IEnumerable<IBatchStatus> statuses, JobStatisticsService statisticsService) 
			: base(caseServiceContext, helper, dataProviderFactory, serializer, appDomainRdoSynchronizerFactoryFactory, jobHistoryService, jobHistoryErrorService, jobManager, statuses, statisticsService)
		{
		}

	    protected override IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
		{
			Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
			
			IDataSynchronizer sourceProvider = _appDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}

	    internal override void ExecuteImport(IEnumerable<FieldMap> fieldMap, string sourceConfiguration, string destinationConfiguration, List<string> entryIDs,
	        SourceProvider sourceProviderRdo, DestinationProvider destinationProvider, Job job)
		{
            ExportUsingSavedSearchSettings sourceSettings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(sourceConfiguration);

            ImportSettings destinationSettings = JsonConvert.DeserializeObject<ImportSettings>(destinationConfiguration);

	        var exportSettings = new FilesDestinationProvider.Core.ExportSettings
	        {
	            ExportedObjArtifactId = sourceSettings.SavedSearchArtifactId,
	            WorkspaceId = sourceSettings.SourceWorkspaceArtifactId,
                ExportFilesLocation = sourceSettings.Fileshare,
                OverwriteFiles = sourceSettings.OverwriteFiles,
                CopyFileFromRepository =  sourceSettings.CopyFileFromRepository,
	            SelViewFieldIds = fieldMap.Select(item => int.Parse(item.SourceField.FieldIdentifier)).ToList(),
	            ArtifactTypeId = destinationSettings.ArtifactTypeId
	        };

	        new ExportProcessRunner().StartWith(exportSettings);
		}
	}
}
