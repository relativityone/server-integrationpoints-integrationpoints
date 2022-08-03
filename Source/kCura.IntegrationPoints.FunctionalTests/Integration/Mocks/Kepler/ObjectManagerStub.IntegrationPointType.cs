using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Match = System.Text.RegularExpressions.Match;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    partial class ObjectManagerStub
    {
        private void SetupIntegrationPointType()
        {
            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(
                    q => IsIntegrationPointTypeRequest(q)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
                {
                    QueryResult result = GetRelativityObjectsForRequest(x => x.IntegrationPointTypes,
                        IntegrationPointTypeFilter, workspaceId, request, length);
                    return Task.FromResult(result);
                });
        }

        private bool IsIntegrationPointTypeRequest(QueryRequest request)
            => request.ObjectType.Guid == ObjectTypeGuids.IntegrationPointTypeGuid;

        private bool IsIntegrationPointTypeByIdentifierCondition(string condition, out string identifier)
        {
            identifier = null;
            if (!string.IsNullOrEmpty(condition))
            {
                Match match = Regex.Match(condition,
            $"'{IntegrationPointTypeFields.Identifier}' == '(.*)'");

                if (match.Success)
                {
                    identifier = match.Groups[1].Value;
                    return true;
                }
            }    
            return false;
        }

        private IList<IntegrationPointTypeTest> IntegrationPointTypeFilter(QueryRequest request, IList<IntegrationPointTypeTest> list)
        {
            if (IsIntegrationPointTypeByIdentifierCondition(request.Condition, out string identifier))
            {
                return list.Where(x => x.Identifier == identifier).ToList();
            }

            return new List<IntegrationPointTypeTest>();
        }
    }
}
