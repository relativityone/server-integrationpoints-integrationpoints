using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class CalculationChecker : ICalculationChecker
    {
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;
        private readonly IAPILog _logger;

        public CalculationChecker(IRelativityObjectManagerFactory relativityObjectManagerFactory, IAPILog logger)
        {
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
            _logger = logger;
        }

        public async Task<CalculationState> MarkAsCalculating(int workspaceId, int integrationPointId)
        {
            CalculationState calculationState = new CalculationState
            {
                IsCalculating = true
            };

            await UpdateCalculationStateValue(workspaceId, integrationPointId, calculationState).ConfigureAwait(false);
            return calculationState;
        }

        public async Task<CalculationState> MarkCalculationAsFinished(int workspaceId, int integrationPointId, DocumentsStatistics statistics)
        {
            CalculationState calculationState = await GetCalculationStateValue(workspaceId, integrationPointId).ConfigureAwait(false);

            if (calculationState == null)
            {
                return new CalculationState
                {
                    IsCalculating = false,
                    ErrorMessage = "ERROR: Could not find calculation state"
                };
            }

            if (!calculationState.IsCalculating)
            {
                _logger.LogError("Could not finish calculation for Integration Point {integrationPointId} as calculation is not in progress", integrationPointId);
                calculationState.ErrorMessage = "ERROR: No ongoing calculation";
                return calculationState;
            }

            calculationState.IsCalculating = false;
            calculationState.CalculationFinishTime = System.DateTime.Now; // should we use 'UtcNow' here?
            calculationState.DocumentStatistics = statistics;

            await UpdateCalculationStateValue(workspaceId, integrationPointId, calculationState).ConfigureAwait(false);

            return calculationState;
        }

        public async Task<CalculationState> GetCalculationState(int workspaceId, int integrationPointId)
        {
            CalculationState calculationState = await GetCalculationStateValue(workspaceId, integrationPointId).ConfigureAwait(false);

            if (calculationState == null)
            {
                return new CalculationState
                {
                    IsCalculating = false,
                    ErrorMessage = "ERROR: Could not find calculation state"
                };
            }

            if (!calculationState.IsCalculating && calculationState.DocumentStatistics == null)
            {
                _logger.LogError("Could not get statistics for Integration Point {integrationPointId}. Calculation marked as finished but no data provided", integrationPointId);
                calculationState.ErrorMessage = "ERROR: Data not found";
                return calculationState;
            }

            return calculationState;
        }

        private async Task<CalculationState> GetCalculationStateValue(int workspaceId, int integrationPointId)
        {
            CalculationState result = null;
            try
            {
                IRelativityObjectManager objectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId);

                QueryRequest queryRequest = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = Guid.Parse(ObjectTypeGuids.IntegrationPoint)
                    },
                    Fields = new FieldRef[] { new FieldRef { Guid = IntegrationPointFieldGuids.CalculationStateGuid } },
                    Condition = $"'Artifact ID' == {integrationPointId}"
                };

                ResultSet<RelativityObject> queryResult = await objectManager.QueryAsync(queryRequest, 0, 1).ConfigureAwait(false);
                string calculationStateValue = queryResult.Items.Single().FieldValues.Single().Value.ToString();

                result = JsonConvert.DeserializeObject<CalculationState>(calculationStateValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not get calculation state for Integration Point {integrationPointId}", integrationPointId);
                return result;
            }

            return result;
        }

        private async Task UpdateCalculationStateValue(int workspaceId, int integrationPointId, CalculationState currentState)
        {
            try
            {
                IRelativityObjectManager objectManager = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId);

                string fieldContent = JsonConvert.SerializeObject(currentState);
                IList<FieldRefValuePair> fieldValues = new[]
                {
                    new FieldRefValuePair
                    {
                        Field = new FieldRef { Guid = IntegrationPointFieldGuids.CalculationStateGuid },
                        Value = fieldContent
                    },
                };

                await objectManager.UpdateAsync(integrationPointId, fieldValues, ExecutionIdentity.System).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not set calculation state for Integration Point {integrationPointId}", integrationPointId);
            }
        }
    }
}
