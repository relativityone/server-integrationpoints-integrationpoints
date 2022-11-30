using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
    internal sealed class Progress : IProgress
    {
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IAPILog _logger;
        private readonly int _syncConfigurationArtifactId;
        private readonly int _workspaceArtifactId;

        private static readonly Guid ProgressObjectTypeGuid = new Guid(SyncProgressGuids.ProgressObjectTypeGuid);

        private static readonly Guid OrderGuid = new Guid(SyncProgressGuids.OrderGuid);
        private static readonly Guid StatusGuid = new Guid(SyncProgressGuids.StatusGuid);
        private static readonly Guid ExceptionGuid = new Guid(SyncProgressGuids.ExceptionGuid);
        private static readonly Guid MessageGuid = new Guid(SyncProgressGuids.MessageGuid);

        private const string _NAME_FIELD_NAME = "Name";
        private const string _PARENT_OBJECT_FIELD_NAME = "SyncConfiguration";

        private Progress(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger, int workspaceArtifactId, int syncConfigurationArtifactId, string name,
            int artifactId, int order, SyncJobStatus status)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _logger = logger;
            _workspaceArtifactId = workspaceArtifactId;
            _syncConfigurationArtifactId = syncConfigurationArtifactId;

            Name = name;
            ArtifactId = artifactId;
            Order = order;
            Status = status;
        }

        private Progress(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger, int workspaceArtifactId, int artifactId)
            : this(serviceFactoryForAdmin, logger, workspaceArtifactId, 0, string.Empty, artifactId, 0, SyncJobStatus.New)
        {
        }

        private Progress(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger, int workspaceArtifactId, int syncConfigurationArtifactId, string name)
            : this(serviceFactoryForAdmin, logger, workspaceArtifactId, syncConfigurationArtifactId, name, 0, 0, SyncJobStatus.New)
        {
        }

        public int ArtifactId { get; private set; }

        public string Name { get; private set; }

        public int Order { get; private set; }

        public SyncJobStatus Status { get; private set; }

        public async Task SetStatusAsync(SyncJobStatus status)
        {
            string description = status.GetDescription();
            await UpdateFieldValueAsync(StatusGuid, description).ConfigureAwait(false);
            Status = status;
        }

        public string Exception { get; private set; }

        public async Task SetExceptionAsync(Exception exception)
        {
            string exceptionString = exception?.ToString();
            await UpdateFieldValueAsync(ExceptionGuid, exceptionString).ConfigureAwait(false);
            Exception = exceptionString;
        }

        public string Message { get; private set; }

        public async Task SetMessageAsync(string message)
        {
            await UpdateFieldValueAsync(MessageGuid, message).ConfigureAwait(false);
            Message = message;
        }

        private async Task CreateAsync()
        {
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                CreateRequest request = new CreateRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = ProgressObjectTypeGuid
                    },
                    ParentObject = new RelativityObjectRef
                    {
                        ArtifactID = _syncConfigurationArtifactId
                    },
                    FieldValues = new[]
                    {
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Name = _NAME_FIELD_NAME
                            },
                            Value = Name
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = OrderGuid
                            },
                            Value = Order
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef
                            {
                                Guid = StatusGuid
                            },
                            Value = Status.GetDescription()
                        }
                    }
                };

                CreateResult result = await objectManager.CreateAsync(_workspaceArtifactId, request).ConfigureAwait(false);

                ArtifactId = result.Object.ArtifactID;
            }
        }

        private async Task ReadAsync()
        {
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                // Do not use ReadAsync here. More details: REL-366692
                QueryRequest request = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        Guid = ProgressObjectTypeGuid
                    },
                    Fields = GetFieldRefsForQuery(),
                    Condition = $"'ArtifactID' == {ArtifactId}",
                    IncludeNameInQueryResult = true
                };
                QueryResult queryResult = await objectManager.QueryAsync(_workspaceArtifactId, request, start: 0, length: 1).ConfigureAwait(false);
                if (!queryResult.Objects.Any())
                {
                    throw new SyncException($"Progress ArtifactID: {ArtifactId} not found.");
                }
                PopulateProgressProperties(queryResult.Objects.Single());
            }
        }

        private async Task<bool> QueryAsync()
        {
            var request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ProgressObjectTypeGuid
                },
                Condition = $"'{_NAME_FIELD_NAME}' == '{Name}' AND '{_PARENT_OBJECT_FIELD_NAME}' == {_syncConfigurationArtifactId}",
                Fields = GetFieldRefsForQuery(),
                IncludeNameInQueryResult = true
            };

            bool objectExists = false;
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, request, start: 1, length: 1).ConfigureAwait(false);
                if (result.Objects.Count > 0)
                {
                    PopulateProgressProperties(result.Objects[0]);
                    objectExists = true;
                }
            }
            return objectExists;
        }

        private async Task<IReadOnlyCollection<IProgress>> QueryAllAsync()
        {
            var progresses = new ConcurrentBag<IProgress>();
            var queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ProgressObjectTypeGuid
                },
                Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {_syncConfigurationArtifactId}",
                IncludeNameInQueryResult = true
            };
            try
            {
                using (var objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue).ConfigureAwait(false);
                    if (result.TotalCount > 0)
                    {
                        IEnumerable<int> progressIds = result.Objects.Select(x => x.ArtifactID);

                        Parallel.ForEach(progressIds, progressArtifactId =>
                        {
                            var progress = new Progress(_serviceFactoryForAdmin, _logger, _workspaceArtifactId, progressArtifactId);
                            progress.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                            progresses.Add(progress);
                        });
                    }
                }
            }
            catch (Exception progressQueryAllException)
            {
                _logger.LogError(progressQueryAllException, "Failed to retrieve all progress information for workspace {WorkspaceArtifactID} and sync configuration object {SyncConfigArtifactID}.",
                    _workspaceArtifactId, _syncConfigurationArtifactId);
            }
            return progresses;
        }

        private static IEnumerable<FieldRef> GetFieldRefsForQuery()
        {
            IEnumerable<FieldRef> fields = new[]
            {
                new FieldRef
                {
                    Name = _NAME_FIELD_NAME
                },
                new FieldRef
                {
                    Guid = OrderGuid
                },
                new FieldRef
                {
                    Guid = StatusGuid
                },
                new FieldRef
                {
                    Guid = ExceptionGuid
                },
                new FieldRef
                {
                    Guid = MessageGuid
                }
            };
            return fields;
        }

        private void PopulateProgressProperties(RelativityObject relativityObject)
        {
            ArtifactId = relativityObject.ArtifactID;
            Name = relativityObject.Name;
            Order = (int)relativityObject[OrderGuid].Value;
            Status = ((string)relativityObject[StatusGuid].Value).GetEnumFromDescription<SyncJobStatus>();
            Exception = (string)relativityObject[ExceptionGuid].Value;
            Message = (string)relativityObject[MessageGuid].Value;
        }

        private async Task UpdateFieldValueAsync<T>(Guid fieldGuid, T value)
        {
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                UpdateRequest request = UpdateRequestExtensions.CreateForSingleField(ArtifactId, fieldGuid, value);
                await objectManager.UpdateAsync(_workspaceArtifactId, request).ConfigureAwait(false);
            }
        }

        public static async Task<IProgress> CreateAsync(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger, CreateProgressDto createProgressDto)
        {
            Progress progress = new Progress(serviceFactoryForAdmin, logger, createProgressDto.WorkspaceArtifactId, createProgressDto.SyncConfigurationArtifactId, createProgressDto.Name,
                0, createProgressDto.Order, createProgressDto.Status);
            await progress.CreateAsync().ConfigureAwait(false);
            return progress;
        }

        public static async Task<IProgress> GetAsync(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger, int workspaceArtifactId, int artifactId)
        {
            Progress progress = new Progress(serviceFactoryForAdmin, logger, workspaceArtifactId, artifactId);
            await progress.ReadAsync().ConfigureAwait(false);
            return progress;
        }

        public static async Task<IReadOnlyCollection<IProgress>> QueryAllAsync(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger, int workspaceArtifactId, int syncConfigurationArtifactId)
        {
            var progress = new Progress(serviceFactoryForAdmin, logger, workspaceArtifactId, syncConfigurationArtifactId, string.Empty);
            IReadOnlyCollection<IProgress> batches = await progress.QueryAllAsync().ConfigureAwait(false);
            return batches;
        }

        public static async Task<IProgress> QueryAsync(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger, int workspaceArtifactId, int syncConfigurationArtifactId, string name)
        {
            Progress progress = new Progress(serviceFactoryForAdmin, logger, workspaceArtifactId, syncConfigurationArtifactId, name);
            bool exists = await progress.QueryAsync().ConfigureAwait(false);
            return exists ? progress : null;
        }
    }
}
