using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IExporterFactory
	{
		IExporterService BuildExporter(IJobStopManager jobStopManager, FieldMap[] mappedFiles, string config, int savedSearchArtifactId, int onBehalfOfUser);

		List<IBatchStatus> InitializeExportServiceJobObservers(Job job,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
			ISynchronizerFactory synchronizerFactory,
			ISerializer serializer,
			IJobHistoryErrorManager jobHistoryErrorManager,
			FieldMap[] mappedFiles,
			SourceConfiguration configuration,
			JobHistoryErrorDTO.UpdateStatusType updateStatusType,
			IntegrationPoint integrationPoint,
			JobHistory jobHistory,
			string uniqueJobId,
			string userImportApiSettings);
	}
}