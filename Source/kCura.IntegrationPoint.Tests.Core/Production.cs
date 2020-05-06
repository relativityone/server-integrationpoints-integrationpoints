using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Production
	{
		private static ITestHelper Helper => new TestHelper();

		public static async Task<int> CreateAsync(int workspaceID, string productionName)
		{
			using (var objectManager = Helper.CreateProxy<IObjectManager>())
			{
				CreateResult createResult = await objectManager.CreateAsync(workspaceID, new CreateRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						Name = "Production"
					},
					FieldValues = new []
					{
						new FieldRefValuePair()
						{
							Field = new FieldRef()
							{
								Name = "Name"
							},
							Value = productionName
						}
					}
				}).ConfigureAwait(false);
				if (createResult.EventHandlerStatuses.All(x => x.Success))
				{
					return createResult.Object.ArtifactID;
				}
				else
				{
					Debugger.Break();
					return -1;
				}
			}
		}

		public static async Task DeleteAsync(int workspaceID, int productionID)
		{
			using (var objectManager = Helper.CreateProxy<IObjectManager>())
			{
				DeleteResult deleteResult = await objectManager.DeleteAsync(workspaceID, new DeleteRequest()
				{
					Object = new RelativityObjectRef()
					{
						ArtifactID = productionID
					}
				}).ConfigureAwait(false);
			}
		}
	}
}