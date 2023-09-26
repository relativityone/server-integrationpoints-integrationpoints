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
        private void SetupUser()
        {
            Mock.Setup(x => x.QuerySlimAsync(It.IsAny<int>(),
                    It.Is<QueryRequest>(r => IsUserQuery(r)), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
            {
                List<RelativityObject> foundObjects = GetUsers(request);

                QueryResultSlim result = new QueryResultSlim();
                result.Objects = foundObjects.Take(length).Select(x => ToSlim(x, request.Fields)).ToList();
                result.TotalCount = result.ResultCount = result.Objects.Count;

                return Task.FromResult(result);
            }
            );
        }

        private bool IsUserQuery(QueryRequest query)
        {
            return query.ObjectType.ArtifactTypeID == (int)ArtifactType.User;
        }

        private List<RelativityObject> GetUsers(QueryRequest request)
        {
            List<RelativityObject> foundObjects = new List<RelativityObject>();

            if (Relativity.TestContext.User != null)
            {
                if (IsArtifactIdCondition(request.Condition, out int artifactId))
                {
                    List<UserFake> user = new List<UserFake>
                    {
                        new UserFake
                        {
                            Artifact =
                            {
                                ArtifactId = Relativity.TestContext.User.ArtifactId,
                                ArtifactType = ArtifactType.User.ToString()
                            }
                        }
                    };
                    AddRelativityObjectsToResult(user
                        , foundObjects);
                }
            }

            return foundObjects;
        }
    }
}
