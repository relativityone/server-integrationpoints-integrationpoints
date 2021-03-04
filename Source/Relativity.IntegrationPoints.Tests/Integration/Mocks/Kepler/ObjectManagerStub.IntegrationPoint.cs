using System.Threading.Tasks;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
	    public void SetupIntegrationPoint(WorkspaceTest workspace, IntegrationPointTest integrationPoint)
	    {
		    Mock.Setup(x => x.ReadAsync(workspace.ArtifactId, It.Is<ReadRequest>(r =>
				    r.Object.ArtifactID == integrationPoint.ArtifactId)))
			    .Returns(() =>
				    {
					    var result = Database.IntegrationPoints.Exists(x => x.ArtifactId == integrationPoint.ArtifactId)
						    ? new ReadResult {Object = new RelativityObject()}
						    : new ReadResult {Object = null};

					    return Task.FromResult(result);
				    }

				);
		}
    }


}
