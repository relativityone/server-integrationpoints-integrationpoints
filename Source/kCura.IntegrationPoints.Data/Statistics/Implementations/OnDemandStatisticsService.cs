using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class OnDemandStatisticsService : IOnDemandStatisticsService
    {
        private readonly IRelativityObjectManagerFactory _objectManagerFactory;
        private readonly IDateTime _dateTime;
        private readonly IAPILog _logger;

        public OnDemandStatisticsService(IRelativityObjectManagerFactory objectManagerFactory, IDateTime dateTime, IAPILog logger)
        {
            _objectManagerFactory = objectManagerFactory;
            _dateTime = dateTime;
            _logger = logger;
        }

        public async Task<CalculationState> MarkAsCalculating(int workspaceId, int integrationPointId)
        {
            CalculationState calculationState = new CalculationState
            {
                Status = CalculationStatus.InProgress
            };

            bool updated = await UpdateCalculationStateValue(workspaceId, integrationPointId, calculationState).ConfigureAwait(false);

            if (!updated)
            {
                calculationState.Status = CalculationStatus.Error;
            }

            return calculationState;
        }

        public async Task<CalculationState> MarkCalculationAsFinished(int workspaceId, int integrationPointId, DocumentsStatistics statistics)
        {
            CalculationState calculationState = GetCalculationState(workspaceId, integrationPointId);

            if (calculationState.Status == CalculationStatus.Error)
            {
                return calculationState;
            }

            if (calculationState.Status != CalculationStatus.InProgress)
            {
                _logger.LogError("Could not finish calculation for Integration Point {integrationPointId} as calculation is not in progress", integrationPointId);
                calculationState.Status = CalculationStatus.Error;
                return calculationState;
            }

            calculationState.Status = CalculationStatus.Completed;
            statistics.CalculatedOn = _dateTime.UtcNow.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
            calculationState.DocumentStatistics = statistics;

            await UpdateCalculationStateValue(workspaceId, integrationPointId, calculationState).ConfigureAwait(false);

            return calculationState;
        }

        public CalculationState GetCalculationState(int workspaceId, int integrationPointId)
        {
            CalculationState result;
            try
            {
                IEnumerable<Guid> integrationPointFields = new[]
                {
                    IntegrationPointFieldGuids.CalculationStateGuid
                };

                IRelativityObjectManager objectManager = _objectManagerFactory.CreateRelativityObjectManager(workspaceId);
                IntegrationPoint integrationPoint = objectManager.Read<IntegrationPoint>(integrationPointId, integrationPointFields);
                if (string.IsNullOrWhiteSpace(integrationPoint.CalculationState))
                {
                    result = new CalculationState
                    {
                        Status = CalculationStatus.New
                    };
                }
                else
                {
                    result = JsonConvert.DeserializeObject<CalculationState>(integrationPoint.CalculationState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not get calculation state for Integration Point {integrationPointId}", integrationPointId);
                result = new CalculationState { Status = CalculationStatus.Error };
                return result;
            }

            return result;
        }

        private async Task<bool> UpdateCalculationStateValue(int workspaceId, int integrationPointId, CalculationState currentState)
        {
            try
            {
                string fieldContent = JsonConvert.SerializeObject(currentState);
                IList<FieldRefValuePair> fieldValues = new[]
                {
                    new FieldRefValuePair
                    {
                        Field = new FieldRef { Guid = IntegrationPointFieldGuids.CalculationStateGuid },
                        Value = fieldContent
                    },
                };

                IRelativityObjectManager objectManager = _objectManagerFactory.CreateRelativityObjectManager(workspaceId);
                return await objectManager.UpdateAsync(integrationPointId, fieldValues, ExecutionIdentity.System).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not set calculation state for Integration Point {integrationPointId} = {@currentState}", integrationPointId, currentState);
                return false;
            }
        }
    }
}
