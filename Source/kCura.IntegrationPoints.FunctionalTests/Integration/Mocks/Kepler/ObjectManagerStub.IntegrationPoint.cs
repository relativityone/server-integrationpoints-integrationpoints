using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
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

        private void SetupIntegrationPoint()
        {
            IList<IntegrationPointTest> IntegrationPointsByCondition(QueryRequest request, IList<IntegrationPointTest> list)
            {
                if (IsProviderCondition(request.Condition, out int sourceId, out int destinationId))
                {
                    return list.Where(x => x.SourceProvider == sourceId && x.DestinationProvider == destinationId).ToList();
                }
                return new List<IntegrationPointTest>();
            }

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(),
                   It.Is<QueryRequest>(r => IsIntegrationPointQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
               .Returns((int workspaceId, QueryRequest request, int start, int length) =>
               {
                   QueryResult result = GetRelativityObjectsForRequest(
                    x => x.IntegrationPoints,
                    IntegrationPointsByCondition,
                    workspaceId,
                    request,
                    length);
                   return Task.FromResult(result);
               }
           );
        }

        private bool IsIntegrationPointQuery(QueryRequest query) => query.ObjectType.Guid == ObjectTypeGuids.IntegrationPointGuid;

        private bool IsProviderCondition(string condition, out int sourceId, out int destinationId)
        {
            System.Text.RegularExpressions.Match match = Regex.Match(condition,
                $"'{IntegrationPointFields.SourceProvider}' == (.*) AND '{IntegrationPointFields.DestinationProvider}' == (.*)");
            
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out sourceId);
                int.TryParse(match.Groups[2].Value, out destinationId);                
                return true;
            }
            sourceId = destinationId = 0;
            return false;
        }
    }
}
