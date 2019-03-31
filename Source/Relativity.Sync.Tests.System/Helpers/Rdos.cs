using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal static class Rdos
	{
		public static async Task<int> CreateJobHistoryInstance(ServiceFactory serviceFactory, int workspaceId)
		{
			using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Value = Guid.NewGuid().ToString(),
							Field = new FieldRef
							{
								Name = "Name"
							}
						}
					},
					ObjectType = new ObjectTypeRef
					{
						Guid = new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9")
					}
				};
				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> CreateSyncConfigurationInstance(ServiceFactory serviceFactory, int workspaceId, int jobHistoryId)
		{
			using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57")
					},
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = jobHistoryId
					}
				};
				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}
	}
}