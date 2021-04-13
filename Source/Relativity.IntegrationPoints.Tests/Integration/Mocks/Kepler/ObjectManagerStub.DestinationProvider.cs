using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		public void SetupDestinationProvider(WorkspaceTest database, DestinationProviderTest destinationProvider)
		{
			Mock.Setup(x => x.ReadAsync(destinationProvider.WorkspaceId, It.Is<ReadRequest>(r =>
					r.Object.ArtifactID == destinationProvider.ArtifactId)))
				.Returns((int workspaceId, ReadRequest request) =>
					{
						ReadResult result = database.DestinationProviders.FirstOrDefault(
							x => x.ArtifactId == request.Object.ArtifactID) != null
							? new ReadResult { Object = destinationProvider.ToRelativityObject() }
							: new ReadResult { Object = null };

						return Task.FromResult(result);
					}
				);
		}
	}
}
