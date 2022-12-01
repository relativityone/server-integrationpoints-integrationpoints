using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private IList<IntegrationPointProfileTest> IntegrationPointProfileFilter(QueryRequest request, IList<IntegrationPointProfileTest> list)
        {
            return list;
        }
        
        private void SetupIntegrationPointProfile()
        {
            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), 
                It.Is<QueryRequest>(r => IsIntegrationPointProfileQueryRequest(r)), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                request.Condition = request.Condition == null ? "" : request.Condition;
                QueryResult result = GetRelativityObjectsForRequest(x => x.IntegrationPointProfiles, IntegrationPointProfileFilter, workspaceId, request, length);
                return Task.FromResult(result);
            });
        }

        private void SetupIntegrationPointProfileLongTextStreaming()
        {
            List<Guid> possibleFiledsGuids = new List<Guid> { 
                IntegrationPointProfileFieldGuids.DestinationConfigurationGuid, 
                IntegrationPointProfileFieldGuids.SourceConfigurationGuid, 
                IntegrationPointProfileFieldGuids.FieldMappingsGuid 
            };

            Mock.Setup(x => x.StreamLongTextAsync(
                    It.IsAny<int>(),
                    It.IsAny<RelativityObjectRef>(),
                    It.Is<FieldRef>(f => possibleFiledsGuids.Contains(f.Guid.Value))))
                .Returns((int workspaceId, RelativityObjectRef objectRef, FieldRef fieldRef) =>
                {
                    var workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);

                    RelativityObject obj = workspace.IntegrationPointProfiles
                        .First(x => x.ArtifactId == objectRef.ArtifactID)
                        .ToRelativityObject();
                    var tmp = obj.FieldValues.Single(x => x.Field.Guids.Single() == fieldRef.Guid.GetValueOrDefault());

                    return Task.FromResult<IKeplerStream>(new KeplerResponseStream(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(obj.FieldValues.Single(x =>
                            x.Field.Guids.Single() == fieldRef.Guid.GetValueOrDefault()).Value.ToString())
                    }));
                });     
        }

        private bool IsIntegrationPointProfileQueryRequest(QueryRequest x)
        {
            return x.ObjectType.Guid.HasValue && x.ObjectType.Guid.Value.Equals(ObjectTypeGuids.IntegrationPointProfileGuid);
        }
    }
}
