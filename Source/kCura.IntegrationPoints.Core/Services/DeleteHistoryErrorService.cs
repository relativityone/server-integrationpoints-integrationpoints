using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteHistoryErrorService : IDeleteHistoryErrorService
	{
		private readonly IRSAPIServiceFactory _rsapiServiceFactory;

		public DeleteHistoryErrorService(IRSAPIServiceFactory rsapiServiceFactory)
		{
			_rsapiServiceFactory = rsapiServiceFactory;
		}

		public void DeleteErrorAssociatedWithHistories(List<int> historiesId, int workspaceArtifactId)
		{
			IRSAPIService rsapiService = _rsapiServiceFactory.Create(workspaceArtifactId);

			// TODO remove
			//var qry = new Query<RDO>
			//{
			//	Fields = FieldValue.NoFields,
			//	Condition = new ObjectCondition(JobHistoryErrorFields.JobHistory, ObjectConditionEnum.AnyOfThese, historiesId)
			//};

			var query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = Guid.Parse(ObjectTypeGuids.JobHistoryError)
				},
				Fields = Enumerable.Empty<FieldRef>(),
				Condition = CreateCondition(historiesId)
			};

			List<JobHistoryError> result = rsapiService.RelativityObjectManager.Query<JobHistoryError>(query);
			List<int> allJobHistoryError = result.Select(x => x.ArtifactId).ToList();
			
			rsapiService.JobHistoryErrorLibrary.Delete(allJobHistoryError);
		}

		private string CreateCondition(List<int> historiesId)
		{
			string historiesIdList = string.Join(",", historiesId.Select(x => x.ToString()));
			return $"'{JobHistoryErrorFields.JobHistory}' IN OBJECT [{historiesIdList}]";
		}
	}
}