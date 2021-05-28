using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
    {
	    private void SetupIntegrationPointLongTextStreaming()
	    {
		    Mock.Setup(x => x.StreamLongTextAsync(
					It.IsAny<int>(),
					It.IsAny<RelativityObjectRef>(),
					It.IsAny<FieldRef>()))
				.Returns((int workspaceId, RelativityObjectRef objectRef, FieldRef fieldRef) =>
				{
					var workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);
						
						RelativityObject obj = workspace.IntegrationPoints
							.First(x => x.ArtifactId == objectRef.ArtifactID)
							.ToRelativityObject();

						return Task.FromResult<IKeplerStream>(new KeplerResponseStream(new HttpResponseMessage(HttpStatusCode.OK)
						{
							Content = new StringContent(obj.FieldValues.Single(x => 
								x.Field.Guids.Single() == fieldRef.Guid.GetValueOrDefault()).Value.ToString())
						}));
					});

			
	    }
	}
}
