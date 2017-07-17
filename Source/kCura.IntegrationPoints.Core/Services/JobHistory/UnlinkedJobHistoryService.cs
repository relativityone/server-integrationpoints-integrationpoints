using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class UnlinkedJobHistoryService : IUnlinkedJobHistoryService
	{
		private readonly IRSAPIServiceFactory _rsapiServiceFactory;

		public UnlinkedJobHistoryService(IRSAPIServiceFactory rsapiServiceFactory)
		{
			_rsapiServiceFactory = rsapiServiceFactory;
		}

		public List<int> FindUnlinkedJobHistories(int workspaceArtifactId)
		{
			var rsapiService = _rsapiServiceFactory.Create(workspaceArtifactId);

			Query<RDO> query = new Query<RDO>
			{
				Fields = FieldValue.NoFields,
				Condition = new NotCondition(new ObjectsCondition(new Guid(JobHistoryFieldGuids.IntegrationPoint), ObjectsConditionEnum.IsSet))
			};
			return rsapiService.JobHistoryLibrary.Query(query).Select(x => x.ArtifactId).ToList();
		}
	}
}