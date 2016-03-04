using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobHistoryService
	{
		private ICaseServiceContext _context;
		public JobHistoryService(ICaseServiceContext context)
		{
			_context = context;
		}

		public Data.JobHistory GetRdo(Guid batchInstance)
		{
			var query = new Query<Relativity.Client.DTOs.RDO>();
			query.ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.JobHistory);
			query.Condition = new TextCondition(Guid.Parse(Data.JobHistoryFieldGuids.BatchInstance), TextConditionEnum.EqualTo, batchInstance.ToString());
			query.Fields = this.GetFields();
			Data.JobHistory jobHistory = _context.RsapiService.JobHistoryLibrary.Query(query).SingleOrDefault(); //there should only be one!

			return jobHistory;
		}

		public Data.JobHistory CreateRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUTC)
		{
			Data.JobHistory jobHistory = null;

			try { jobHistory = GetRdo(batchInstance); }
			catch { }

			if (jobHistory == null)
			{
				jobHistory = new Data.JobHistory();
				jobHistory.Name = integrationPoint.Name;
				jobHistory.IntegrationPoint = new[] { integrationPoint.ArtifactId };
				jobHistory.BatchInstance = batchInstance.ToString();
				jobHistory.JobStatus = JobStatusChoices.JobHistoryPending;
				jobHistory.RecordsImported = 0;
				jobHistory.RecordsWithErrors = 0;

				ImportSettings setting = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);
				jobHistory.DestinationWorkspace = String.Format("{0} [Id::{1}]", _context.GetWorkspaceName(setting.CaseArtifactId), setting.CaseArtifactId);

				if (startTimeUTC.HasValue) jobHistory.StartTimeUTC = startTimeUTC.Value;

				int artifactId = _context.RsapiService.JobHistoryLibrary.Create(jobHistory);
				jobHistory.ArtifactId = artifactId;
			}

			return jobHistory;
		}

		public void UpdateRdo(Data.JobHistory jobHistory)
		{
			_context.RsapiService.JobHistoryLibrary.Update(jobHistory);
		}

		protected List<FieldValue> GetFields()
		{
			return (from field in (BaseRdo.GetFieldMetadata(typeof(Data.JobHistory)).Values).ToList()
							select new FieldValue(field.FieldGuid)).ToList();
		}
	}
}
