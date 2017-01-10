using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Injection;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.BatchProcess;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	[SynchronizedTask]
	public class ImageSyncManager : SyncManager
	{
		public ImageSyncManager(ICaseServiceContext caseServiceContext,
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
			: base(caseServiceContext,
				providerFactory,
				jobManager,
				jobService,
				helper,
				integrationPointService,
				serializer,
				guidService,
				jobHistoryService,
				jobHistoryErrorService,
				scheduleRuleFactory,
				managerFactory,
				contextContainerFactory,
				batchStatuses)
		{
		}

		public override int BatchTask(Job job, IEnumerable<string> sourceRecords)
		{
			int count = 0;
			List<string> list = new List<string>();

			string previousDocumentId = string.Empty;
			bool overBatchSize = false;

			foreach (var currentRecord in sourceRecords)
			{
				if (!string.IsNullOrEmpty(currentRecord))
				{
					string currentDocumentId = GetDocumentId(currentRecord);

					if (overBatchSize && currentDocumentId != previousDocumentId)
					{
						count += list.Count;
						CreateBatchJob(job, list);
						list = new List<string>();
						overBatchSize = false;
					}

					list.Add(currentRecord);
					if (!overBatchSize && list.Count == BatchSize)
					{
						overBatchSize = true;
					}	
					previousDocumentId = currentDocumentId;
				}
			}

			if (list.Any())
			{
				count += list.Count;
				CreateBatchJob(job, list);
			}
			return count;
		}

		private string GetDocumentId(string currentRecord)
		{
			return currentRecord.Split(OpticonInfo.OPTICON_RECORD_DELIMITER)[OpticonInfo.DOCUMENT_ID_FIELD_INDEX];
		}
	}
}