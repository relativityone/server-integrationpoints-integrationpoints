using System.Threading.Tasks;
using Moq;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
	    private void SetupSavedSearch()
	    {
		    Mock.Setup(x => x.QueryAsync(It.IsAny<int>(), 
				    It.Is<QueryRequest>(r => IsSavedSearchQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
			    .Returns((int workspaceId, QueryRequest request, int start, int length) =>
				    {
					    QueryResult result = GetRelativityObjectsForRequest(x => x.SavedSearches, null, workspaceId, request, length);
					    return Task.FromResult(result);
				    }
			    );

		    Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(),
				    It.Is<QueryRequest>(r => IsSavedSearchQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
			    .Returns((int workspaceId, QueryRequest request, int start, int length) =>
			    {
				    QueryResultSlim result = GetQuerySlimsForRequest(x=>x.SavedSearches, null, workspaceId, request, length);
				    return Task.FromResult(result);
			    });
	    }

	    private bool IsSavedSearchQuery(QueryRequest query)
	    {
		    return query.ObjectType.ArtifactTypeID == (int) ArtifactType.Search;
	    }
    }
}
