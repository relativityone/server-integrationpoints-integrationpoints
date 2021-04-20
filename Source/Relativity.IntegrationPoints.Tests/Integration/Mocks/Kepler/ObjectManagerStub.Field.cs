using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private void SetupDocumentFields()
        {
            bool IsDocumentFieldCondition(string condition)
            {
                return condition == @"'Object Type Artifact Type Id' == OBJECT 10";
            }

            IList<FieldTest> DocumentFieldFilter(QueryRequest request, IList<FieldTest> list)
            {
                if (IsDocumentFieldCondition(request.Condition))
                {
                    return list.Where(x => x.IsDocumentField).ToList();
                }

                return new List<FieldTest>();
            }
            
            Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), 
                    It.Is<QueryRequest>(r => IsFieldQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
                    {
                        QueryResult result = GetRelativityObjectsForRequest(x => x.Fields, DocumentFieldFilter, workspaceId, request, length);
                        return Task.FromResult(result);
                    }
                );

            Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsFieldQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
                {
                    QueryResultSlim result = GetQuerySlimsForRequest(x=>x.Fields, DocumentFieldFilter, workspaceId, request, length);
                    return Task.FromResult(result);
                });
        }

        private bool IsFieldQuery(QueryRequest r)
        {
	        return r.ObjectType.ArtifactTypeID == (int) ArtifactType.Field;
        }
    }
}