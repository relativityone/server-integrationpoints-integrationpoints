using System;
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
    internal class NewCustomProviderFlowCheck : INewCustomProviderFlowCheck
    {
        private readonly IToggleProvider _toggleProvider;
        private readonly IRelativityObjectManager _objectManager;
        private readonly ISerializer _serializer;
        private readonly IAPILog _log;

        public NewCustomProviderFlowCheck(
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
                return _toggleProvider.IsEnabled<EnableImportApiV2ForCustomProvidersToggle>()
                    && !await IsEntityObjectImportAsync(integrationPoint.DestinationConfiguration).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error occurred during New Custom Provider flow usage checking.");
                return false;
            }

        }

        private async Task<bool> IsEntityObjectImportAsync(string configuration)
        {
            ImportSettings settings = _serializer.Deserialize<ImportSettings>(configuration);
            var request = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.ObjectType },
                Condition = $"'Artifact Type ID' == {settings.ArtifactTypeId}"
            };

            var results = await _objectManager.QueryAsync(request, ExecutionIdentity.System).ConfigureAwait(false);
            if (results.Count == 0)
            {
                return false;
            }

            return results.First().Guids.Contains(ObjectTypeGuids.Entity);
        }
    }
}
