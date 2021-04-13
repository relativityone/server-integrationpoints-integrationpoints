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
	    public void SetupDocumentFields(WorkspaceTest workspace)
	    {
		    Mock.Setup(x => x.QueryAsync(workspace.ArtifactId, It.Is<QueryRequest>(q =>
					    q.ObjectType.ArtifactTypeID == (int) ArtifactType.Field &&
					    q.Condition == $"'Object Type Artifact Type Id' == OBJECT {(int) ArtifactType.Document}"),
				    It.IsAny<int>(), It.IsAny<int>()))
			    .Returns((int workspaceId, QueryRequest request, int start, int length) =>
			    {
				    IList<FieldTest> searches = workspace.Fields
					    .Where(x => x.WorkspaceId == workspaceId && x.IsDocumentField).ToList();

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
