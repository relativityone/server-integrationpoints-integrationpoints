using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
	public partial class ObjectManagerStub
    {
	    public void SetupIntegrationPoint(InMemoryDatabase database, IntegrationPointTest integrationPoint)
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
				.ReturnsAsync(new KeplerResponseStream(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(integrationPoint.FieldMappings)
				}));

			Mock.Setup(x => x.UpdateAsync(integrationPoint.WorkspaceId, It.Is<UpdateRequest>(r =>
				r.Object.ArtifactID == integrationPoint.ArtifactId))).ReturnsAsync(
				new UpdateResult()
				{
					EventHandlerStatuses = new List<EventHandlerStatus>()
				});
	    }
	}
}
