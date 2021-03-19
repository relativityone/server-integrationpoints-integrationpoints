using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
    {
	    public void SetupIntegrationPoints(InMemoryDatabase database, IEnumerable<IntegrationPointTest> integrationPoints)
	    {
		    foreach (IntegrationPointTest integrationPoint in integrationPoints)
		    {
				Mock.Setup(x => x.ReadAsync(integrationPoint.WorkspaceId, It.Is<ReadRequest>(r =>
						r.Object.ArtifactID == integrationPoint.ArtifactId)))
					.Returns((int workspaceId, ReadRequest request) =>
						{
							ReadResult result = database.IntegrationPoints.FirstOrDefault(
									x => x.ArtifactId == request.Object.ArtifactID) != null
								? new ReadResult { Object = integrationPoint.ToRelativityObject() }
								: new ReadResult { Object = null };

							return Task.FromResult(result);
						}
					);

				Mock.Setup(x => x.StreamLongTextAsync(
						integrationPoint.WorkspaceId,
						It.Is<RelativityObjectRef>(objectRef => objectRef.ArtifactID == integrationPoint.ArtifactId),
						It.Is<FieldRef>(field => field.Guid == IntegrationPointTest.FieldsMappingGuid)))
					.ReturnsAsync(new KeplerStream(new MemoryStream(Encoding.Unicode.GetBytes(integrationPoint.FieldMappings))));
		    }
	    }
	}
}
