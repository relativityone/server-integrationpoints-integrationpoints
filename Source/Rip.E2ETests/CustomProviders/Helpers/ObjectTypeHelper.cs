using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class ObjectTypeHelper
	{
		public static async Task<int> GetObjectTypeArtifactIdByNameAsync(
			IRelativityObjectManager objectManager,
			string artifactName
		)
		{
			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.ObjectType },
				Fields = new List<FieldRef>
				{
					new FieldRef() {Name = "Artifact Type ID" }
				},
				Condition = $"'Name' == '{artifactName}'"
			};

			List<RelativityObject> ObjectTypeArtifactTypeID = await objectManager.QueryAsync(queryRequest).ConfigureAwait(false);
			return ObjectTypeArtifactTypeID.Single().FieldValues.GetNumericFieldValue("Artifact Type ID");
		}
	}
}