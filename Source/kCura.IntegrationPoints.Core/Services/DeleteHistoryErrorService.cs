using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

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
			var rsapiService = _rsapiServiceFactory.Create(workspaceArtifactId);

			var qry = new Query<RDO>
			{
				Fields = FieldValue.NoFields,
				Condition = new ObjectCondition(JobHistoryErrorFields.JobHistory, ObjectConditionEnum.AnyOfThese, historiesId)
			};
			var result = rsapiService.JobHistoryErrorLibrary.Query(qry);
			var allJobHistoryError = result.Select(x => x.ArtifactId).ToList();

			rsapiService.JobHistoryErrorLibrary.Delete(allJobHistoryError);
		}
	}
}