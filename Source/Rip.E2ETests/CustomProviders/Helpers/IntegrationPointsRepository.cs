using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services;
using Relativity.Services.Objects.DataContracts;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class IntegrationPointsRepository
	{
		public static async Task<int> SaveIntegrationPointAsync(
			ITestHelper testHelper,
			IRelativityObjectManager objectManager,
			CreateIntegrationPointRequest integrationPointCreateRequest)
		{
			IntegrationPointModel createdIntegrationPoint;
			using (var integrationPointManager = testHelper.CreateProxy<IIntegrationPointManager>())
			{
				createdIntegrationPoint = await integrationPointManager
					.CreateIntegrationPointAsync(integrationPointCreateRequest)
					.ConfigureAwait(false);
			}

			// workaround for serialization issue
			var fieldsToUpdate = new List<FieldRefValuePair>
			{
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = IntegrationPointFieldGuids.SourceConfigurationGuid
					},
					Value = integrationPointCreateRequest.IntegrationPoint.SourceConfiguration
				}
			};

			objectManager.Update(createdIntegrationPoint.ArtifactId, fieldsToUpdate);


			return createdIntegrationPoint.ArtifactId;
		}

		public static async Task RunIntegrationPointAsync(
			ITestHelper testHelper,
			int workspaceID,
			int integrationPointId)
		{
			using (var integrationPointManager = testHelper.CreateProxy<IIntegrationPointManager>())
			{
				await integrationPointManager
					.RunIntegrationPointAsync(workspaceID, integrationPointId)
					.ConfigureAwait(false);
			}
		}
	}
}
