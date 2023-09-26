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
        private void SetupSavedSearchDocuments()
        {
            IList<DocumentFake> DocumentsFilter(QueryRequest request, IList<DocumentFake> list)
            {
                bool hasNatives = request.Condition.Contains($"'{DocumentFieldsConstants.HasNativeFieldGuid}' == true");
                bool hasImages = request.Condition.Contains($"'{DocumentFieldsConstants.HasImagesFieldName}' == CHOICE");
                bool hasFields = request.Fields.Any();

                List<DocumentFake> documents = list.Where(x =>
                        x.HasNatives == hasNatives &&
                        x.HasImages == hasImages &&
                        x.HasFields == hasFields)
                    .ToList();
                return documents;
            }

            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsSavedSearchDocumentsQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResult result = GetRelativityObjectsForRequest(x => x.Documents, DocumentsFilter, workspaceId, request, length);
                return Task.FromResult(result);
            }
            );

            Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsSavedSearchDocumentsQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                QueryResultSlim result = GetQuerySlimsForRequest(x=> x.Documents, DocumentsFilter, workspaceId, request, length);
                return Task.FromResult(result);
            }
            );
        }

        private bool IsSavedSearchDocumentsQuery(QueryRequest query)
        {
            if (query.Condition == null)
            {
                return false;
            }
            Match savedSearchMatch = Regex.Match(query.Condition, @"'ArtifactId' IN SAVEDSEARCH (\d+).*");
            return savedSearchMatch.Success;
        }
    }
}
