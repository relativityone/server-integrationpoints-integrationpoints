using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
    internal sealed class RelativitySourceJobTagRepository : IRelativitySourceJobTagRepository
    {
        private const string _NAME_FIELD_NAME = "Name";
        private const string _SENSITIVE_DATA_REMOVED = "[Sensitive user data has been removed]";

        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IAPILog _logger;

        private static readonly Guid JobHistoryNameGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
        private static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
        private static readonly Guid RelativitySourceJobTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

        public RelativitySourceJobTagRepository(ISourceServiceFactoryForUser serviceFactoryForUser, IAPILog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _logger = logger;
        }

        public async Task<RelativitySourceJobTag> ReadAsync(int destinationWorkspaceArtifactId, int jobHistoryArtifactId, CancellationToken token)
        {
            using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = RelativitySourceJobTypeGuid,
                    },
                    Condition = $"'JobHistoryArtifactId' == {jobHistoryArtifactId}",
                    Fields = GetFieldRefs(),
                    IncludeNameInQueryResult = true
                };

                QueryResult result = await objectManager.QueryAsync(destinationWorkspaceArtifactId, queryRequest, 0, 1).ConfigureAwait(false);

                return result.TotalCount > 0
                    ? PrepareSourceJobTag(result.Objects.First())
                    : null;
            }
        }

        public async Task<RelativitySourceJobTag> CreateAsync(int destinationWorkspaceArtifactId, RelativitySourceJobTag sourceJobTag, CancellationToken token)
        {
            _logger.LogVerbose($"Creating {nameof(RelativitySourceJobTag)} in destination workspace artifact ID: {{destinationWorkspaceArtifactId}} Source case tag artifact ID: {{sourceCaseTagArtifactId}}",
                destinationWorkspaceArtifactId, sourceJobTag.SourceCaseTagArtifactId);
            using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                CreateRequest request = new CreateRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = RelativitySourceJobTypeGuid,
                    },
                    ParentObject = new RelativityObjectRef { ArtifactID = sourceJobTag.SourceCaseTagArtifactId },
                    FieldValues = CreateFieldValues(sourceJobTag.Name, sourceJobTag.JobHistoryArtifactId, sourceJobTag.JobHistoryName)
                };

                CreateResult result;
                try
                {
                    result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request).ConfigureAwait(false);
                }
                catch (ServiceException ex)
                {
                    request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
                    _logger.LogError(ex, $"Service call failed while creating {nameof(RelativitySourceJobTag)}: {{request}}", request);
                    throw new RelativitySourceJobTagRepositoryException($"Service call failed while creating {nameof(RelativitySourceJobTag)}: {request}", ex);
                }
                catch (Exception ex)
                {
                    request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
                    _logger.LogError(ex, $"Failed to create {nameof(RelativitySourceJobTag)}: {{request}}", request);
                    throw new RelativitySourceJobTagRepositoryException($"Failed to create {nameof(RelativitySourceJobTag)} in workspace {sourceJobTag.SourceCaseTagArtifactId}", ex);
                }

                RelativitySourceJobTag createdTag = new RelativitySourceJobTag()
                {
                    ArtifactId = result.Object.ArtifactID,
                    Name = sourceJobTag.Name,
                    SourceCaseTagArtifactId = sourceJobTag.SourceCaseTagArtifactId,
                    JobHistoryArtifactId = sourceJobTag.JobHistoryArtifactId,
                    JobHistoryName = sourceJobTag.JobHistoryName
                };

                return createdTag;
            }
        }

        private IEnumerable<FieldRef> GetFieldRefs()
        {
            return new FieldRef[]
            {
                new FieldRef
                {
                    Guid = JobHistoryIdFieldGuid
                },
                new FieldRef
                {
                    Guid = JobHistoryNameGuid
                }
            };
        }

        private static RelativitySourceJobTag PrepareSourceJobTag(RelativityObject obj)
        {
            return new RelativitySourceJobTag
            {
                ArtifactId = obj.ArtifactID,
                Name = obj.Name,
                JobHistoryArtifactId = (int)obj.FieldValues.First(x => x.Field.Guids.Contains(JobHistoryIdFieldGuid)).Value,
                JobHistoryName = (string)obj.FieldValues.First(x => x.Field.Guids.Contains(JobHistoryNameGuid)).Value,
                SourceCaseTagArtifactId = obj.ParentObject.ArtifactID
            };
        }

        private IEnumerable<FieldRefValuePair> CreateFieldValues(string sourceJobTagName, int jobHistoryArtifactId, string jobHistoryName)
        {
            FieldRefValuePair[] pairs =
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Name = _NAME_FIELD_NAME },
                    Value = sourceJobTagName
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = JobHistoryIdFieldGuid },
                    Value = jobHistoryArtifactId
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = JobHistoryNameGuid },
                    Value = jobHistoryName
                }
            };

            return pairs;
        }

        private IEnumerable<FieldRefValuePair> RemoveSensitiveUserData(IEnumerable<FieldRefValuePair> fieldValues)
        {
            fieldValues.First(fieldValue => fieldValue.Field.Name == _NAME_FIELD_NAME).Value = _SENSITIVE_DATA_REMOVED;
            fieldValues.First(fieldValue => fieldValue.Field.Guid == JobHistoryNameGuid).Value = _SENSITIVE_DATA_REMOVED;

            return fieldValues;
        }
    }
}
