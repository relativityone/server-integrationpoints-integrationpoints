using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private void SetupDestinationProvider()
        {
            IList<DestinationProviderTest> DestinationProviderByCondition(QueryRequest request, IList<DestinationProviderTest> list)
            {
                if (IsDestinationIdentifierCondition(request.Condition, out string identifier))
                {
                    return list.Where(x => x.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return new List<DestinationProviderTest>();
            }

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(),
                 It.Is<QueryRequest>(r => IsDestinationProviderQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
             .Returns((int workspaceId, QueryRequest request, int start, int length) =>
             {
                 QueryResult result = GetRelativityObjectsForRequest(
                      x => x.DestinationProviders,
                      DestinationProviderByCondition,
                      workspaceId,
                      request,
                      length);
                 return Task.FromResult(result);
             });
        }

        private bool IsDestinationProviderQuery(QueryRequest query) =>
            query.ObjectType.Guid == ObjectTypeGuids.DestinationProviderGuid;

        private bool IsDestinationIdentifierCondition(string condition, out string identifierValue)
        {
            if (condition != null)
            {
                System.Text.RegularExpressions.Match match = Regex.Match(condition,
                $"'{DestinationProviderFields.Identifier}' == '(.*)'");

                if (match.Success)
                {
                    identifierValue = match.Groups[1].Value;
                    return true;
                }
            }
            identifierValue = null;
            return false;
        }

    }
}
