using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
	    public void SetupIntegrationPoint(Workspace workspace, IntegrationPoint integrationPoint)
	    {
		    Mock.Setup(x => x.ReadAsync(workspace.ArtifactId, It.Is<ReadRequest>(r =>
				    r.Object.ArtifactID == integrationPoint.ArtifactId)))
			    .ReturnsAsync(new ReadResult
			    {
				    Object = new RelativityObject()
			    });
	    }
    }
}
