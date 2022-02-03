using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using Match = System.Text.RegularExpressions.Match;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private void SetupProductionDocuments()
	    {
           
            IList<DocumentTest> Filter(QueryRequest request, IList<DocumentTest> list)
            {
                bool hasNatives = request.Condition.Contains($"'{ProductionConsts.WithNativesFieldName}' == true");
                bool hasImages = false;
                bool hasFields = request.Fields.Any();

                List<DocumentTest> documents = list.Where(x =>
                        x.HasNatives == hasNatives &&
                        x.HasImages == hasImages &&
                        x.HasFields == hasFields)
                    .ToList();
                return documents;
            }


            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsProductionDocumentsQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
                    {
                        QueryResult result = GetRelativityObjectsForRequest(x => x.Documents, Filter, workspaceId, request, length);
                        return Task.FromResult(result);
                    }
                );

            Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsProductionDocumentsQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
                    {
                        QueryResultSlim result = GetQuerySlimsForRequest(x => x.Documents, Filter, workspaceId, request, length);
                        return Task.FromResult(result);
                    }
                );
		}

	    private bool IsProductionDocumentsQuery(QueryRequest query)
	    {
            Match savedSearchMatch = Regex.Match(query.Condition, @"'ProductionSet' == OBJECT (\d+).*");
            return savedSearchMatch.Success;
        }
    }
}
