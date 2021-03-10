using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
	    public void SetupIntegrationPoints(InMemoryDatabase database, IEnumerable<IntegrationPointTest> integrationPoints)
	    {
		    foreach (var integrationPoint in integrationPoints)
		    {
				Mock.Setup(x => x.ReadAsync(integrationPoint.WorkspaceId, It.Is<ReadRequest>(r =>
						r.Object.ArtifactID == integrationPoint.ArtifactId)))
					.Returns((int workspaceId, ReadRequest request) =>
						{
							var result = database.IntegrationPoints.FirstOrDefault(
									x => x.ArtifactId == request.Object.ArtifactID) != null
								? new ReadResult { Object = new RelativityObject() }
								: new ReadResult { Object = null };

							return Task.FromResult(result);
						}
					);
			}
	    }
	}


}
