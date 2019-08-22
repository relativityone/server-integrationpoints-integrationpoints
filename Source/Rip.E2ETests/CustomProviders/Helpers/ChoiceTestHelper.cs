using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class ChoiceTestHelper
	{
		public static async Task<int> GetIntegrationPointsChoiceArtifactIDAsync(
			IRelativityObjectManager objectManager,
			Guid choiceGuid
			)
		{
			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = (int)ArtifactType.Code
				},
				Condition = "'Object Type' IN ['Integration Point']"
			};
			List<RelativityObject> integrationPointChoices = await objectManager.QueryAsync(queryRequest).ConfigureAwait(false);
			return integrationPointChoices.Single(choice => choice.Guids.Contains(choiceGuid)).ArtifactID;
		}
	}
}
