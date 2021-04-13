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
	    public void SetupIntegrationPoint(WorkspaceTest database, IntegrationPointTest integrationPoint)
	    {
			Mock.Setup(x => x.ReadAsync(integrationPoint.WorkspaceId, It.Is<ReadRequest>(r =>
					r.Object.ArtifactID == integrationPoint.ArtifactId)))
				.Returns((int workspaceId, ReadRequest request) =>
					{
						IntegrationPointTest readIntegrationPoint = database.IntegrationPoints
							.FirstOrDefault(x => x.ArtifactId == request.Object.ArtifactID);

						ReadResult result = readIntegrationPoint != null
							? new ReadResult { Object = readIntegrationPoint.ToRelativityObject() }
							: new ReadResult { Object = null };

						return Task.FromResult(result);
					}
				);
			
			Mock.Setup(x => x.StreamLongTextAsync(
					integrationPoint.WorkspaceId,
					It.Is<RelativityObjectRef>(objectRef => objectRef.ArtifactID == integrationPoint.ArtifactId),
					It.Is<FieldRef>(field => field.Guid == IntegrationPointTest.FieldsMappingGuid)))
				.Returns((int workspaceId, RelativityObjectRef objectRef, FieldRef fieldRef) =>
					{
						RelativityObject obj = database.IntegrationPoints
							.First(x => x.ArtifactId == objectRef.ArtifactID)
							.ToRelativityObject();

						return Task.FromResult<IKeplerStream>(new KeplerResponseStream(new HttpResponseMessage(HttpStatusCode.OK)
						{
							Content = new StringContent(obj.FieldValues.Single(x => 
								x.Field.Guids.Single() == fieldRef.Guid.GetValueOrDefault()).Value.ToString())
						}));
					});

			Mock.Setup(x => x.UpdateAsync(integrationPoint.WorkspaceId, It.Is<UpdateRequest>(r =>
				r.Object.ArtifactID == integrationPoint.ArtifactId))).ReturnsAsync(
				new UpdateResult()
				{
					EventHandlerStatuses = new List<EventHandlerStatus>()
				});
	    }
	}
}
