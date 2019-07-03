﻿using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IExportServiceObserversFactory
	{
		List<IBatchStatus> InitializeExportServiceJobObservers(
			Job job,
			ITagsCreator tagsCreator,
			ITagSavedSearchManager tagSavedSearchManager,
			ISynchronizerFactory synchronizerFactory,
			ISerializer serializer,
			IJobHistoryErrorManager jobHistoryErrorManager,
			IJobStopManager jobStopManager,
			ISourceWorkspaceTagCreator sourceWorkspaceTagCreator,
			FieldMap[] mappedFields,
			SourceConfiguration configuration,
			JobHistoryErrorDTO.UpdateStatusType updateStatusType,
			JobHistory jobHistory,
			string uniqueJobID,
			string userImportApiSettings);
	}
}
