using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using Relativity;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices
{
    public class EntityFullNameObjectManagerService : IEntityFullNameObjectManagerService
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IAPILog _logger;

        public EntityFullNameObjectManagerService(IKeplerServiceFactory serviceFactory, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public async Task<int> GetFullNameArtifactId(int workspaceId)
        {
            const string fullName = EntityFieldNames.FullName;

            try
            {
                using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    QueryRequest queryRequest = new QueryRequest
                    {
                        ObjectType = new ObjectTypeRef
                        {
                            ArtifactTypeID = (int)ArtifactType.Field
                        },
                        Fields = new[]
                        {
                            new FieldRef
                            {
                                Name = "ArtifactID"
                            }
                        },
                        Condition = $"'DisplayName' == '{fullName}'"
                    };

                    QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceId, queryRequest, 0, 1).ConfigureAwait(false);

                    if (result == null || !result.Objects.Any())
                    {
                        throw new NotFoundException($"{fullName} field not found in workspace ID: {workspaceId}");
                    }

                    int fullNameArtifactId = (int)result.Objects.Single().Values.Single();

                    _logger.LogInformation("{fieldName} field retrieved, ArtifactID: {artifactId}", fullName, fullNameArtifactId);

                    return fullNameArtifactId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query for {fieldName} in workspace ID: {workspaceId}", fullName, workspaceId);
                throw;
            }
        }

        public async Task<bool> IsEntityAsync(int workspaceId, int artifactTypeId)
        {
            try
            {
                using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>())
                {
                    QueryRequest queryRequest = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef
                        {
                            ArtifactTypeID = (int)ArtifactType.ObjectType
                        },
                        Fields = new[]
                        {
                            new FieldRef
                            {
                                Name = "DescriptorArtifactTypeID"
                            }
                        },
                        Condition = "'Name' == 'Entity'"
                    };

                    QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceId, queryRequest, 0, 1).ConfigureAwait(false);

                    if (result == null || !result.Objects.Any())
                    {
                        throw new NotFoundException($"Entity Object Type Artifact ID: {artifactTypeId} not found in workspace ID: {workspaceId}");
                    }

                    int entityArtifactTypeId = (int)result.Objects.Single().Values.Single();

                    return artifactTypeId == entityArtifactTypeId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check whether Artifact Type ID: {artifactTypeId} is of type Entity", artifactTypeId);
                throw;
            }
        }
    }
}