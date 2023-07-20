using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub
    {
        private void SetupObjectType()
        {
            Mock.Setup(x => x.QuerySlimAsync(
                    It.IsAny<int>(),
                    It.Is<QueryRequest>(
                    q => IsObjectTypeQuery(q)),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new QueryResultSlim
                {
                    Objects = new List<RelativityObjectSlim> { new RelativityObjectSlim() },
                    TotalCount = 1
                });

            Mock.Setup(x => x.QueryAsync(
                    It.IsAny<int>(),
                    It.Is<QueryRequest>(
                    q => IsObjectTypeQuery(q)),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            Guids = new List<Guid>(),
                            FieldValues = new List<FieldValuePair>(),
                            ParentObject = new RelativityObjectRef(),
                            Name = string.Empty
                        }
                    },
                    TotalCount = 1
                });

            Mock.Setup(x => x.QueryAsync(
                    It.IsAny<int>(),
                    It.Is<QueryRequest>(
                    q =>
                        IsObjectTypeQuery(q) && q.Condition == $"'DescriptorArtifactTypeID' IN [{Const.Entity._ENTITY_TYPE_ARTIFACT_ID}]"),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                    {
                        new RelativityObject
                        {
                            Guids = new List<Guid> { ObjectTypeGuids.Entity },
                            FieldValues = new List<FieldValuePair>(),
                            ParentObject = new RelativityObjectRef(),
                            Name = string.Empty
                        }
                    },
                    TotalCount = 1
                });

            Mock.Setup(x => x.QueryAsync(
                    It.IsAny<int>(),
                    It.Is<QueryRequest>(
                        q => IsObjectTypeQuery(q) &&
                             IsArtifactTypeIdCondition(q.Condition)),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((int workspaceId, QueryRequest request, int start, int length) =>
                {
                    List<RelativityObject> foundObjects = GetObjectForArtifactTypeId(workspaceId, request);
                    QueryResult result = new QueryResult();
                    result.Objects = foundObjects.Take(length).ToList();
                    result.TotalCount = result.ResultCount = result.Objects.Count;
                    return Task.FromResult(result);
                });

            Mock.Setup(x => x.QuerySlimAsync(
                    It.IsAny<int>(),
                    It.Is<QueryRequest>(
                        q =>
                            IsObjectTypeQuery(q) &&
                            q.Condition == "'Name' == 'Entity'" &&
                            q.Fields.FirstOrDefault().Name == "DescriptorArtifactTypeID"),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new QueryResultSlim
                    {
                        Objects = new List<RelativityObjectSlim>
                        {
                            new RelativityObjectSlim
                            {
                                Values = new List<object> { -1 }
                            }
                        },
                        TotalCount = 1
                    }
                );
        }

        private bool IsObjectTypeQuery(QueryRequest query)
        {
            return query.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType;
        }

        private bool IsArtifactTypeIdCondition(string condition)
        {
            var match = Regex.Match(condition, @"'Artifact[ ]?Type[ ]?ID' == (\d+)");
            return match.Success;
        }

        private bool IsArtifactTypeIdCondition(string condition, out int artifactTypeId)
        {
            var match = Regex.Match(condition, @"'Artifact[ ]?Type[ ]?ID' == (\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int extractedArtifactTypeId))
            {
                artifactTypeId = extractedArtifactTypeId;
                return true;
            }

            artifactTypeId = -1;
            return false;
        }

        private List<RelativityObject> GetObjectForArtifactTypeId(int workspaceId, QueryRequest request)
        {
            List<RelativityObject> foundObjects = new List<RelativityObject>();

            if (IsArtifactTypeIdCondition(request.Condition, out int artifactTypeId))
            {
                WorkspaceTest workspace = Relativity.Workspaces.Where(x => x.ArtifactId == workspaceId).FirstOrDefault();
                AddRelativityObjectsToResult(
                    workspace.ObjectTypes.Where(x => x.ArtifactTypeId == artifactTypeId)
                    , foundObjects);
            }

            return foundObjects;
        }
    }
}
