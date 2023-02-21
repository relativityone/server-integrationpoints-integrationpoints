using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class CustomProviderFlowCheck : ICustomProviderFlowCheck
    {
        private readonly IToggleProvider _toggleProvider;
        private readonly IRelativityObjectManager _objectManager;
        private readonly ISerializer _serializer;
        private readonly IAPILog _log;

        public CustomProviderFlowCheck(
            IToggleProvider toggleProvider,
            IRelativityObjectManager objectManager,
            ISerializer serializer,
            IAPILog log)
        {
            _toggleProvider = toggleProvider;
            _objectManager = objectManager;
            _serializer = serializer;
            _log = log;
        }

        public async Task<bool> ShouldBeUsedAsync(IntegrationPointDto integrationPoint)
        {
            try
            {
                bool isToggleEnabled = await _toggleProvider.IsEnabledAsync<EnableImportApiV2ForCustomProvidersToggle>().ConfigureAwait(false);

                ImportSettings settings = _serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration);
                bool isEntityImport = await IsEntityObjectImportAsync(settings).ConfigureAwait(false);
                bool isDocumentImport = IsDocumentImport(settings);

                bool shouldBeUsed = isToggleEnabled && !isEntityImport && isDocumentImport;

                _log.LogInformation("Checking if IAPI 2.0 should be used for Custom Provider flow: {shouldBeUsed}, because: is toggle enabled - {isToggleEnabled}; is Entity import - {isEntityImport}",
                    shouldBeUsed, isToggleEnabled, isEntityImport);

                return shouldBeUsed;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error occurred during New Custom Provider flow usage checking.");
                return false;
            }
        }

        private bool IsDocumentImport(ImportSettings settings)
        {
            return settings.ArtifactTypeId == (int)ArtifactType.Document;
        }

        private async Task<bool> IsEntityObjectImportAsync(ImportSettings settings)
        {
            var request = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.ObjectType },
                Condition = $"'Artifact Type ID' == {settings.ArtifactTypeId}"
            };

            List<RelativityObject> results = await _objectManager.QueryAsync(request, ExecutionIdentity.System).ConfigureAwait(false);
            if (results.Count == 0)
            {
                return false;
            }

            return results.First().Guids.Contains(ObjectTypeGuids.Entity);
        }
    }
}
