using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <inheritdoc/>
    internal class RdoImportApiRunner : IImportApiRunner
    {
        private readonly IRdoImportSettingsBuilder _importSettingsBuilder;
        private readonly IImportApiService _importApiService;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IAPILog _logger;

        /// <summary>
        /// Parameterless constructor for tests purposes only.
        /// </summary>
        public RdoImportApiRunner()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentImportApiRunner"/> class.
        /// </summary>
        /// <param name="importSettingsBuilder">The builder able to create desired ImportAPI configuration.></param>
        /// <param name="importApiService">The service responsible for ImportAPI calls.</param>
        /// <param name="logger">The logger.</param>
        public RdoImportApiRunner(IRdoImportSettingsBuilder importSettingsBuilder, IImportApiService importApiService, IRelativityObjectManager objectManager, IAPILog logger)
        {
            _importSettingsBuilder = importSettingsBuilder;
            _importApiService = importApiService;
            _objectManager = objectManager;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RunImportJobAsync(ImportJobContext importJobContext, IntegrationPointInfo integrationPoint)
        {
            _logger.LogInformation("ImportApiRunner for rdo flow started. ImportJobId: {jobId}", importJobContext.JobHistoryGuid);

            RdoImportConfiguration configuration = await CreateConfiguration(integrationPoint).ConfigureAwait(false);

            await _importApiService.CreateImportJobAsync(importJobContext).ConfigureAwait(false);

            await _importApiService.ConfigureRdoImportApiJobAsync(importJobContext, configuration).ConfigureAwait(false);

            await _importApiService.StartImportJobAsync(importJobContext).ConfigureAwait(false);
        }

        private async Task<RdoImportConfiguration> CreateConfiguration(IntegrationPointInfo integrationPoint)
        {
            RdoImportConfiguration configuration;
            if (integrationPoint.ShouldGenerateFullNameIdentifierField)
            {
                var clonedFieldMappings = new List<IndexedFieldMap>();
                foreach (var fieldMapping in integrationPoint.FieldMap)
                {
                    IndexedFieldMap clonedFieldMapping = fieldMapping.Clone();
                    clonedFieldMappings.Add(clonedFieldMapping);
                }

                IndexedFieldMap clonedLastName = clonedFieldMappings.First(x => x.DestinationFieldName == EntityFieldNames.LastName);

                clonedLastName.FieldMap.FieldMapType = FieldMapTypeEnum.None;
                clonedLastName.FieldMap.SourceField.IsIdentifier = false;
                clonedLastName.FieldMap.DestinationField.IsIdentifier = false;
                string fullNameArtifactId = await GetFullNameArtifactId();
                var fullNameField = new FieldEntry
                {
                    DisplayName = EntityFieldNames.FullName,
                    IsIdentifier = true,
                    IsRequired = false,
                    FieldIdentifier = fullNameArtifactId
                };

                clonedFieldMappings.Add(new IndexedFieldMap(
                    new FieldMap
                    {
                        DestinationField = fullNameField,
                        FieldMapType = FieldMapTypeEnum.Identifier,
                        SourceField = fullNameField
                    },
                    integrationPoint.FieldMap.Count));

                configuration = _importSettingsBuilder.Build(integrationPoint.DestinationConfiguration, clonedFieldMappings);
            }
            else
            {
                configuration = _importSettingsBuilder.Build(integrationPoint.DestinationConfiguration, integrationPoint.FieldMap);
            }

            return configuration;
        }

        private async Task<string> GetFullNameArtifactId()
        {
            List<RelativityObjectSlim> result = await _objectManager.QuerySlimAsync(new QueryRequest
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
            }).ConfigureAwait(false);

            if (result == null || result.Count < 1)
            {
                throw new NotFoundException($"{EntityFieldNames.FullName} not found in Destination Workspace");
            }

            string fullNameArtifactId = result.Single().Values.Single().ToString();

            _logger.LogInformation(
                "{FullName} field retrieved with Object Manager, ArtifactID = {artifactId}",
                EntityFieldNames.FullName,
                fullNameArtifactId);
            return fullNameArtifactId;
        }
    }
}
