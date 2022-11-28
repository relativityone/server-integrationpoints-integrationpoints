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
using Match = System.Text.RegularExpressions.Match;

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

                    object fieldValue = obj.FieldValues.Single(x => x.Field.Guids.Single() == fieldRef.Guid.GetValueOrDefault()).Value;
                    return Task.FromResult<IKeplerStream>(new KeplerResponseStream(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent((fieldValue ?? String.Empty).ToString())
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
                if (IsGetAllIntegrationPointsCondition(request))
                {
                    return list;
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

        private bool IsProviderCondition(string condition, out int sourceProviderId, out int destinationProviderId)
        {
            Match match = Match.Empty;
            if (!string.IsNullOrEmpty(condition))
            {
                match = Regex.Match(condition,
                    $"'{IntegrationPointFields.SourceProvider}' == (.*) AND '{IntegrationPointFields.DestinationProvider}' == (.*)");
            }

            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out sourceProviderId);
                int.TryParse(match.Groups[2].Value, out destinationProviderId);
                return true;
            }
            sourceProviderId = destinationProviderId = 0;
            return false;
        }

        private static bool IsGetAllIntegrationPointsCondition(QueryRequest request)
        {
            IEnumerable<Guid> guids = BaseRdo.GetFieldMetadata(typeof(IntegrationPoint))
                .Values
                .Select(x => x.FieldGuid);

            return guids.All(x => request.Fields.Any(y => y.Guid == x));
        }
    }
}
