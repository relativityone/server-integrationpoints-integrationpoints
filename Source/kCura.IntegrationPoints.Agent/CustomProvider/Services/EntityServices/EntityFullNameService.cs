using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices
{
    public class EntityFullNameService : IEntityFullNameService
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IAPILog _logger;

        public EntityFullNameService(IKeplerServiceFactory serviceFactory, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public async Task HandleFullNameMappingIfNeededAsync(IntegrationPointInfo integrationPoint)
        {
            if (await ShouldHandleFullNameAsync(integrationPoint.SourceWorkspaceArtifactId, integrationPoint.DestinationConfiguration.ArtifactTypeId, integrationPoint.FieldMap).ConfigureAwait(false))
            {
                await EnrichFieldMapWithFullNameAsync(integrationPoint).ConfigureAwait(false);
            }
        }

        public string FormatFullName(Dictionary<string, IndexedFieldMap> destinationFieldNameToFieldMapDictionary, IDataReader reader)
        {
            FieldEntry firstNameField = destinationFieldNameToFieldMapDictionary[EntityFieldNames.FirstName].FieldMap.SourceField;
            FieldEntry lastNameField = destinationFieldNameToFieldMapDictionary[EntityFieldNames.LastName].FieldMap.SourceField;

            string firstName = reader[firstNameField.ActualName]?.ToString() ?? string.Empty;
            string lastName = reader[lastNameField.ActualName]?.ToString() ?? string.Empty;

            string fullName = FormatFullName(firstName, lastName);

            return fullName;
        }

        private async Task<bool> ShouldHandleFullNameAsync(int workspaceId, int artifactTypeId, List<IndexedFieldMap> fieldsMap)
        {
            bool isFullNameMapped = fieldsMap.Exists(x => x.DestinationFieldName == EntityFieldNames.FullName);
            bool isEntity = await IsEntityAsync(workspaceId, artifactTypeId).ConfigureAwait(false);

            bool shouldHandleFullName = isEntity && !isFullNameMapped;

            _logger.LogInformation("Should handle full name: {shouldHandleFullName} because is Full Name field mapped: {isFullNameMapped}, is Entity: {isEntity}", shouldHandleFullName, isFullNameMapped, isEntity);

            return shouldHandleFullName;
        }

        private async Task EnrichFieldMapWithFullNameAsync(IntegrationPointInfo integrationPoint)
        {
            int fullNameArtifactId = await GetFullNameArtifactId(integrationPoint.DestinationConfiguration.CaseArtifactId).ConfigureAwait(false);

            var fullNameField = new FieldEntry
            {
                DisplayName = EntityFieldNames.FullName,
                IsIdentifier = true,
                IsRequired = false,
                FieldIdentifier = fullNameArtifactId.ToString()
            };

            FieldMap fullNameFieldMap = new FieldMap
            {
                FieldMapType = FieldMapTypeEnum.None,
                SourceField = fullNameField,
                DestinationField = fullNameField
            };

            IndexedFieldMap indexedFullNameFieldMap = new IndexedFieldMap(fullNameFieldMap, FieldMapType.EntityFullName, integrationPoint.FieldMap.Count);

            integrationPoint.FieldMap.Add(indexedFullNameFieldMap);
        }

        private static string FormatFullName(string firstName, string lastName)
        {
            string fullName = lastName;

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    fullName += ", ";
                }

                fullName += firstName;
            }

            return fullName;
        }

        private async Task<int> GetFullNameArtifactId(int workspaceId)
        {
            using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryResultSlim result = await objectManager.QuerySlimAsync(
                    workspaceId,
                    new QueryRequest
                    {
                        Fields = new[]
                        {
                            new FieldRef
                            {
                                Name = "ArtifactID"
                            }
                        },
                        ObjectType = new ObjectTypeRef
                        {
                            Name = "Field"
                        },
                        Condition = $"'DisplayName' == '{EntityFieldNames.FullName}'"
                    },
                    0,
                    1).ConfigureAwait(false);

                if (result == null || result.ResultCount < 1)
                {
                    throw new NotFoundException($"{EntityFieldNames.FullName} field not found in Destination Workspace");
                }

                int fullNameArtifactId = (int)result.Objects.Single().Values.Single();

                _logger.LogInformation(
                    "{FullName} field retrieved with Object Manager, ArtifactID = {artifactId}",
                    EntityFieldNames.FullName,
                    fullNameArtifactId);
                return fullNameArtifactId;
            }
        }

        private async Task<bool> IsEntityAsync(int workspaceId, int artifactTypeId)
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
                        Condition = "'Name' == 'Entity'",
                        Fields = new[]
                        {
                            new FieldRef
                            {
                                Name = "DescriptorArtifactTypeID"
                            }
                        }
                    };

                    QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceId, queryRequest, 0, 1).ConfigureAwait(false);

                    if (result == null || result.Objects.Count < 1)
                    {
                        throw new NotFoundException("Entity Object Type DescriptorArtifactTypeID not found.");
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