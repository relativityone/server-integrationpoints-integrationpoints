using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.IntegrationPointRdoService
{
    public class IntegrationPointRdoService : IIntegrationPointRdoService
    {
        private readonly IKeplerServiceFactory _keplerServiceFactory;
        private readonly IAPILog _logger;

        public IntegrationPointRdoService(IKeplerServiceFactory keplerServiceFactory, IAPILog logger)
        {
            _keplerServiceFactory = keplerServiceFactory;
            _logger = logger;
        }

        public async Task TryUpdateLastRuntimeAsync(int workspaceId, int integrationPointId, DateTime lastRuntime)
        {
            try
            {
                await UpdateIntegrationPointAsync(workspaceId, integrationPointId, new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = IntegrationPointFieldGuids.LastRuntimeUTCGuid
                    },
                    Value = lastRuntime
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Integration Point Last Runtime: {lastRuntime}", lastRuntime);
            }
        }

        public async Task TryUpdateHasErrorsAsync(int workspaceId, int integrationPointId, bool hasErrors)
        {
            try
            {
                await UpdateIntegrationPointAsync(workspaceId, integrationPointId, new FieldRefValuePair()
                {
                    Field = new FieldRef()
                    {
                        Guid = IntegrationPointFieldGuids.HasErrorsGuid
                    },
                    Value = hasErrors
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Integration Point Has Errors: {hasErrors}", hasErrors);
            }
        }

        private async Task UpdateIntegrationPointAsync(int workspaceId, int integrationPointId, FieldRefValuePair field)
        {
            using (IObjectManager objectManager = await _keplerServiceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                UpdateRequest updateRequest = new UpdateRequest()
                {
                    Object = new RelativityObjectRef()
                    {
                        ArtifactID = integrationPointId
                    },
                    FieldValues = new[]
                    {
                        field
                    }
                };

                await objectManager.UpdateAsync(workspaceId, updateRequest).ConfigureAwait(false);

                _logger.LogInformation(
                    "Integration Point ID {integrationPointId} has been successfully updated with field: {@field}",
                    integrationPointId,
                    field);
            }
        }
    }
}