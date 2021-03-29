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
	    public void SetupSavedSearch(InMemoryDatabase database, SavedSearchTest savedSearch)
	    {
		    Mock.Setup(x => x.QueryAsync(savedSearch.WorkspaceId, It.Is<QueryRequest>(q =>
				    q.ObjectType.ArtifactTypeID == (int) ArtifactType.Search &&
				    q.Condition == $"'Artifact ID' == {savedSearch.ArtifactId}"), It.IsAny<int>(), It.IsAny<int>()))
			    .Returns((int workspaceId, QueryRequest request, int start, int length) =>
			    {
				    IList<SavedSearchTest> searches = database.SavedSearches
					    .Where(x => x.ArtifactId == savedSearch.ArtifactId).ToList();

				    return Task.FromResult(new QueryResult
				    {
					    Objects = searches.Select(x => x.ToRelativityObject()).ToList(),
					    ResultCount = searches.Count,
					    TotalCount = searches.Count
				    });
			    });
	    }
    }
}
