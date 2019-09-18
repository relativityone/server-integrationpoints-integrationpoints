using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	internal static class ObjectManagerExtensions
	{
		public static async Task<MassDeleteResult> MassDeleteObjectsByArtifactIds(this IObjectManager objectManager, int workspaceId, Guid objectTypeGuid, IEnumerable<int> artifactIds)
		{
			Condition deleteNonSyncProfilesCondition = new WholeNumberCondition("ArtifactID", NumericConditionEnum.In, artifactIds.ToList());

			var massDeleteByCriteriaRequest = new MassDeleteByCriteriaRequest
			{
				ObjectIdentificationCriteria = new ObjectIdentificationCriteria
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = objectTypeGuid
					},
					Condition = deleteNonSyncProfilesCondition.ToQueryString()
				}
			};
			MassDeleteResult massDeleteResult = await objectManager.DeleteAsync(workspaceId, massDeleteByCriteriaRequest).ConfigureAwait(false);

			return massDeleteResult;
		}
	}
}
