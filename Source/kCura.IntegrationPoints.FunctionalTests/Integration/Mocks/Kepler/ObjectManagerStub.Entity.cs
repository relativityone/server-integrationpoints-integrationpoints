using kCura.IntegrationPoints.Core.Contracts.Entity;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private void SetupEntity()
        {
            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(
                q => q.ObjectType.Guid == ObjectTypeGuids.Entity), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResult result = GetRelativityObjectsForRequest(x => x.Entities,
                    EntitiesFilter, workspaceId, request, length);
                return Task.FromResult(result);
            });
        }

        private bool GetEntitiesByUid(string condition, out List<string> entitiesUids)
        {
            System.Text.RegularExpressions.Match match = Regex.Match(condition,
                $@"'{EntityFieldNames.UniqueId}' IN \[(.*)\]");

            if (match.Success)
            {
                entitiesUids = match.Groups[1].Value.Split(',').Select(x => x.Trim('\'')).ToList();
                return true;
            }

            entitiesUids = new List<string>();
            return false;
        }

        private IList<EntityTest> EntitiesFilter(QueryRequest request, IList<EntityTest> list)
        {
            if (GetEntitiesByUid(request.Condition, out List<string> entitiesUids))
            {
                return list.Where(x => entitiesUids.Contains(x.UniqueId)).ToList();
            }

            return new List<EntityTest>();
        }
    }
}
