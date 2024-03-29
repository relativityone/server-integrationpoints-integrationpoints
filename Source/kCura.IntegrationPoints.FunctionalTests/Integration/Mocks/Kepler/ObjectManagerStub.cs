﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using FieldRef = Relativity.Services.Objects.DataContracts.FieldRef;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public partial class ObjectManagerStub : KeplerStubBase<IObjectManager>
    {
        public void Setup()
        {
            Mock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>()))
                .Returns((int workspaceId, CreateRequest request) =>
            {
                RelativityObject createdObject = GetRelativityObject(request);
                AddObjectToDatabase(new ObjectCreationInfo(workspaceId, createdObject, request.ObjectType.Guid));

                return Task.FromResult(new CreateResult()
                {
                    Object = createdObject
                });
            });

            Mock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<MassCreateRequest>()))
                .Returns((int workspaceId, MassCreateRequest request) =>
            {
                List<RelativityObject> createdObjects = GetRelativityObjects(request);

                createdObjects.ForEach(x =>
                    AddObjectToDatabase(new ObjectCreationInfo(workspaceId, x, request.ObjectType.Guid)));

                return Task.FromResult(new MassCreateResult
                {
                    Success = true,
                    Objects = createdObjects.Select(x => new RelativityObjectRef { ArtifactID = x.ArtifactID })
                        .ToList()
                });
            });

            Mock.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
                .Returns((int workspaceId, UpdateRequest request) =>
            {
                try
                {
                    WorkspaceTest workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);
                    RdoTestBase foundRdo = workspace.ReadArtifact(request.Object.ArtifactID);

                    if (foundRdo != null)
                    {
                        RelativityObject relativityObject = GetRelativityObject(request, foundRdo);

                        if (relativityObject.FieldValues.Any(x => x.Field.Name == null))
                        {
                            foundRdo.LoadRelativityObjectByGuid(foundRdo.GetType(), relativityObject);
                        }
                        else
                        {
                            foundRdo.LoadRelativityObjectByName(foundRdo.GetType(),
                                relativityObject);
                        }
                    }

                    return Task.FromResult(new UpdateResult()
                    {
                        EventHandlerStatuses = new List<EventHandlerStatus>()
                    });
                }
                catch (Exception)
                {
                    Debugger.Break();
                    return Task.FromResult<UpdateResult>(null);
                }
            }
            );

            SetupRead();
            SetupJobHistory();
            SetupDocumentFields();
            SetupSavedSearch();
            SetupView();
            SetupUser();
            SetupSavedSearchDocuments();
            SetupProductionDocuments();
            SetupFolderDocuments();
            SetupIntegrationPoint();
            SetupIntegrationPointLongTextStreaming();
            SetupIntegrationPointProfile();
            SetupIntegrationPointProfileLongTextStreaming();
            SetupWorkspace();
            SetupArtifact();
            SetupObjectType();
            SetupSyncConfiguration();
            SetupJobHistoryError();
            SetupIntegrationPointType();
            SetupApplications();
            SetupEntity();
            SetupSourceProvider();
            SetupDestinationProvider();
        }

        private void AddObjectToDatabase(ObjectCreationInfo objectCreationInfo)
        {
            var workspace = Relativity.Workspaces.First(x => x.ArtifactId == objectCreationInfo.WorkspaceId);

            if (objectCreationInfo.ObjectTypeGuid == ObjectTypeGuids.JobHistoryGuid)
            {
                var newRdo = new JobHistoryTest();
                newRdo.LoadRelativityObjectByName<JobHistoryTest>(objectCreationInfo.CreatedObject);
                workspace.JobHistory.Add(newRdo);
            }
            else if (objectCreationInfo.ObjectTypeGuid == ObjectTypeGuids.JobHistoryErrorGuid)
            {
                var newRdo = new JobHistoryErrorTest();
                newRdo.LoadRelativityObjectByGuid<JobHistoryErrorTest>(objectCreationInfo.CreatedObject);
                workspace.JobHistoryErrors.Add(newRdo);
            }
            else if (objectCreationInfo.ObjectTypeGuid == ObjectTypeGuids.IntegrationPointProfileGuid)
            {
                var newRdo = new IntegrationPointProfileTest();
                newRdo.LoadRelativityObjectByName<IntegrationPointProfileTest>(objectCreationInfo.CreatedObject);
                workspace.IntegrationPointProfiles.Add(newRdo);
            }
            else if (objectCreationInfo.ObjectTypeGuid == ObjectTypeGuids.IntegrationPointGuid)
            {
                var newRdo = new IntegrationPointTest();
                newRdo.LoadRelativityObjectByGuid<IntegrationPointTest>(objectCreationInfo.CreatedObject);
                workspace.IntegrationPoints.Add(newRdo);
            }
            else
            {
                Debugger.Break();
                throw new Exception($"Adding of RDO {objectCreationInfo.ObjectTypeGuid} is not yet implemented");
            }
        }

        private void SetupRead()
        {
            Mock.Setup(x => x.ReadAsync(It.IsAny<int>(),
                    It.IsAny<ReadRequest>()))
            .Returns((int workspaceId, ReadRequest request) =>
            {
                RdoTestBase foundRdo;
                if (workspaceId == -1)
                {
                    foundRdo = Relativity.Workspaces.First(x => x.ArtifactId == request.Object.ArtifactID);
                }
                else
                {
                    WorkspaceTest workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);
                    foundRdo = workspace.ReadArtifact(request.Object.ArtifactID);
                }

                ReadResult result = new ReadResult { Object = foundRdo?.ToRelativityObject() };

                return Task.FromResult(result);
            }
            );
        }

        private QueryResultSlim GetQuerySlimsForRequest<T>(Func<WorkspaceTest, IList<T>> collectionGetter,
            Func<QueryRequest, IList<T>, IList<T>> customFilter, int workspaceId,
            QueryRequest request, int length) where T : RdoTestBase
        {
            WorkspaceTest workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);

            List<RelativityObject> foundObjects = FindObjects(collectionGetter, customFilter, request, workspace);

            QueryResultSlim result = new QueryResultSlim();
            result.Objects = foundObjects.Take(length).Select(x => ToSlim(x, request.Fields)).ToList();
            result.TotalCount = result.ResultCount = result.Objects.Count;
            return result;
        }

        private QueryResultSlim GetQuerySlimsForRequest<T>(Func<WorkspaceTest, IList<T>> collectionGetter,
            Func<QueryRequest, IList<T>, int, IList<T>> customFilter, int workspaceId,
            QueryRequest request, int start, int length) where T : RdoTestBase
        {
            WorkspaceTest workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);

            List<RelativityObject> foundObjects = FindObjects(collectionGetter, customFilter, request, workspace, start);

            QueryResultSlim result = new QueryResultSlim();
            result.Objects = foundObjects.Take(length).Select(x => ToSlim(x, request.Fields)).ToList();
            result.TotalCount = result.ResultCount = result.Objects.Count;
            return result;
        }

        private QueryResult GetRelativityObjectsForRequest<T>(Func<WorkspaceTest, IList<T>> collectionGetter,
            Func<QueryRequest, IList<T>, IList<T>> customFilter, int workspaceId,
            QueryRequest request, int length) where T : RdoTestBase
        {
            WorkspaceTest workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);

            List<RelativityObject> foundObjects = FindObjects(collectionGetter, customFilter, request, workspace);

            QueryResult result = new QueryResult();
            result.Objects = foundObjects.Take(length).ToList();
            result.TotalCount = result.ResultCount = result.Objects.Count;

            return result;
        }

        private QueryResult GetRelativityObjectsForRequest<T>(Func<WorkspaceTest, IList<T>> collectionGetter,
            Func<QueryRequest, IList<T>, int, IList<T>> customFilter, int workspaceId,
            QueryRequest request, int start, int length) where T : RdoTestBase
        {
            WorkspaceTest workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceId);

            List<RelativityObject> foundObjects = FindObjects(collectionGetter, customFilter, request, workspace, start);

            foundObjects = foundObjects.Select(x =>
            {
                x.Name = x.FieldValues[1].Value.ToString();
                return x;
            }).ToList();

            QueryResult result = new QueryResult();
            result.Objects = foundObjects.Take(length).ToList();
            result.TotalCount = result.ResultCount = result.Objects.Count;

            return result;
        }

        private RelativityObjectSlim ToSlim(RelativityObject relativityObject, IEnumerable<FieldRef> fields)
        {
            return new RelativityObjectSlim
            {
                // we want to return field values in correct order
                Values = fields.Select(f => relativityObject.FieldValues.First(of => of.Field.Name == f.Name))
                    .Select(x => (object)x).ToList(),
                ArtifactID = relativityObject.ArtifactID
            };
        }

        private List<RelativityObject> FindObjects<T>(
                Func<WorkspaceTest, IList<T>> collectionGetter,
                Func<QueryRequest, IList<T>, IList<T>> customFilter,
                QueryRequest request,
                WorkspaceTest workspace)
            where T : RdoTestBase
        {
            IEnumerable<T> foundObjects = FindObjectsByStandardCondition(collectionGetter, request, workspace);
            if (foundObjects == null)
            {
                foundObjects = customFilter != null
                    ? customFilter(request, collectionGetter(workspace))
                    : Enumerable.Empty<T>();
            }

            return foundObjects.Select(x => x.ToRelativityObject()).ToList();
        }

        private List<RelativityObject> FindObjects<T>(
                Func<WorkspaceTest, IList<T>> collectionGetter,
                Func<QueryRequest, IList<T>, int, IList<T>> customFilter,
                QueryRequest request,
                WorkspaceTest workspace, int start)
            where T : RdoTestBase
        {
            IEnumerable<T> foundObjects = FindObjectsByStandardCondition(collectionGetter, request, workspace);
            if (foundObjects == null)
            {
                foundObjects = customFilter != null
                    ? customFilter(request, collectionGetter(workspace), start)
                    : Enumerable.Empty<T>();
            }

            return foundObjects.Select(x => x.ToRelativityObject()).ToList();
        }

        private IEnumerable<T> FindObjectsByStandardCondition<T>(
                Func<WorkspaceTest, IList<T>> collectionGetter,
                QueryRequest request,
                WorkspaceTest workspace)
            where T : RdoTestBase
        {
            if (request.Condition != null && IsArtifactIdCondition(request.Condition, out int artifactId))
            {
                return collectionGetter(workspace).Where(x => x.ArtifactId == artifactId);
            }
            else if (request.Condition != null && IsArtifactIdListCondition(request.Condition, out int[] artifactIds))
            {
                return collectionGetter(workspace).Where(x => artifactIds.Contains(x.ArtifactId));
            }
            else if (string.IsNullOrEmpty(request.Condition))
            {
                return collectionGetter(workspace);
            }

            return null;
        }

        private static void AddRelativityObjectsToResult<T>(IEnumerable<T> objectsToAdd,
            List<RelativityObject> resultList) where T : RdoTestBase
        {
            resultList.AddRange(objectsToAdd.Select(x => x.ToRelativityObject()));
        }

        private bool IsArtifactIdCondition(string condition, out int artifactId)
        {
            artifactId = -1;
            if (!string.IsNullOrEmpty(condition))
            {
                var match = Regex.Match(condition,
               @"'Artifact[ ]?ID' == (\d+)");

                if (match.Success && int.TryParse(match.Groups[1].Value, out int extractedArtifactId))
                {
                    artifactId = extractedArtifactId;
                    return true;
                }
            }
            return false;
        }

        private bool IsArtifactIdListCondition(string condition, out int[] artifactId)
        {
            artifactId = new int[0];
            if (!string.IsNullOrEmpty(condition))
            {
                var match = Regex.Match(condition,
                @"'Artifact[ ]?ID' in \[(.*)\]");

                if (match.Success)
                {
                    artifactId = match.Groups[1].Value.Split(',').Select(x => int.TryParse(x, out int r) ? r : -1)
                        .Where(x => x > 0)
                        .ToArray();
                    return true;
                }
            }
            return false;
        }

        private List<RelativityObject> GetRelativityObjects(MassCreateRequest request)
        {
            return request.ValueLists.Select(x => new RelativityObject
            {
                ArtifactID = ArtifactProvider.NextId(),
                FieldValues = x.Select((v, i) => new FieldValuePair
                {
                    Field = new Field
                    {
                        Name = request.Fields[i].Name,
                        Guids = new List<Guid> { request.Fields[i].Guid.Value }
                    },
                    Value = v
                }).ToList(),
                ParentObject = request.ParentObject
            }).ToList();
        }


        private RelativityObject GetRelativityObject(CreateRequest request)
        {
            return new RelativityObject()
            {
                ArtifactID = ArtifactProvider.NextId(),
                FieldValues = request.FieldValues.Select(x => new FieldValuePair
                {
                    Field = new Field
                    {
                        Name = x.Field.Name,
                        Guids = x.Field.Guid.HasValue ? new List<Guid> { x.Field.Guid.Value } : new List<Guid>()
                    },
                    Value = x.Value
                }).ToList(),
                ParentObject = request.ParentObject
            };
        }

        private RelativityObject GetRelativityObject(UpdateRequest request, RdoTestBase rdo)
        {
            return new RelativityObject()
            {
                ArtifactID = rdo.ArtifactId,
                FieldValues = request.FieldValues.Select(x => new FieldValuePair
                {
                    Field = new Field
                    {
                        Name = x.Field.Name,
                        Guids = x.Field.Guid.HasValue ? new List<Guid> { x.Field.Guid.Value } : new List<Guid>()
                    },
                    Value = x.Value
                }).ToList(),
                ParentObject = new RelativityObjectRef { ArtifactID = rdo.ParentObjectArtifactId }
            };
        }

        public class ObjectCreationInfo
        {
            public ObjectCreationInfo(int workspaceId, RelativityObject createdObject, Guid? objectTypeGuid)
            {
                WorkspaceId = workspaceId;
                CreatedObject = createdObject;
                ObjectTypeGuid = objectTypeGuid;
            }

            public int WorkspaceId { get; }
            public RelativityObject CreatedObject { get; }
            public Guid? ObjectTypeGuid { get; set; }
        }
    }
}
