using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
	{
		public void SetupSourceProvider(WorkspaceTest workspace, SourceProviderTest sourceProvider)
		{
			Mock.Setup(x => x.ReadAsync(workspace.ArtifactId, It.Is<ReadRequest>(r =>
					r.Object.ArtifactID == sourceProvider.ArtifactId)))
				.Returns((int workspaceId, ReadRequest request) =>
					{
						ReadResult result = workspace.SourceProviders.FirstOrDefault(
							x => x.ArtifactId == request.Object.ArtifactID) != null
							? new ReadResult { Object = sourceProvider.ToRelativityObject() }
							: new ReadResult { Object = null };

						return Task.FromResult(result);
					}
				);
		}
	}
}
