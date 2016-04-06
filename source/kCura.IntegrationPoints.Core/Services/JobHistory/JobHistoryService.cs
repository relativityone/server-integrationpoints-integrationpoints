﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobHistoryService : IJobHistoryService
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IWorkspaceRepository _workspaceRepository;

		public JobHistoryService(ICaseServiceContext caseServiceContext, IWorkspaceRepository workspaceRepository)
		{
			_caseServiceContext = caseServiceContext;
			_workspaceRepository = workspaceRepository;
		}

		public Data.JobHistory GetRdo(Guid batchInstance)
		{
			var query = new Query<Relativity.Client.DTOs.RDO>();
			query.ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.JobHistory);
			query.Condition = new TextCondition(Guid.Parse(Data.JobHistoryFieldGuids.BatchInstance), TextConditionEnum.EqualTo, batchInstance.ToString());
			query.Fields = this.GetFields();
			Data.JobHistory jobHistory = _caseServiceContext.RsapiService.JobHistoryLibrary.Query(query).SingleOrDefault(); //there should only be one!

			return jobHistory;
		}

		public Data.JobHistory CreateRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUTC)
		{
			Data.JobHistory jobHistory = null;

			try
			{
				jobHistory = GetRdo(batchInstance);
			}
			catch
			{
				// ignored
			}

			if (jobHistory == null)
			{
				jobHistory = new Data.JobHistory
				{
					Name = integrationPoint.Name,
					IntegrationPoint = new[] {integrationPoint.ArtifactId},
					BatchInstance = batchInstance.ToString(),
					Status = JobStatusChoices.JobHistoryPending,
					ItemsImported = 0,
					ItemsWithErrors = 0
				};

				ImportSettings setting = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);
				jobHistory.DestinationWorkspace = $"{_workspaceRepository.Retrieve(setting.CaseArtifactId).Name} [Id::{setting.CaseArtifactId}]";

				if (startTimeUTC.HasValue)
				{
					jobHistory.StartTimeUTC = startTimeUTC.Value;
				}

				int artifactId = _caseServiceContext.RsapiService.JobHistoryLibrary.Create(jobHistory);
				jobHistory.ArtifactId = artifactId;
			}

			return jobHistory;
		}

		public void UpdateRdo(Data.JobHistory jobHistory)
		{
			_caseServiceContext.RsapiService.JobHistoryLibrary.Update(jobHistory);
		}

		protected List<FieldValue> GetFields()
		{
			return (from field in (BaseRdo.GetFieldMetadata(typeof(Data.JobHistory)).Values).ToList()
							select new FieldValue(field.FieldGuid)).ToList();
		}
	}
}
