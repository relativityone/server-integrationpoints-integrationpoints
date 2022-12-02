using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class CalculationChecker : ICalculationChecker
    {
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IAPILog _logger;

        public CalculationChecker(IRelativityObjectManager relativityObjectManager, IAPILog logger)
        {
            _relativityObjectManager = relativityObjectManager;
            _logger = logger;
        }

        public async Task<CalculationState> MarkAsCalculating(int integrationPointId)
        {
            CalculationState calculationState = new CalculationState
            {
                Status = CalculationStatus.InProgress
            };

            bool updated = await UpdateCalculationStateValue(integrationPointId, calculationState).ConfigureAwait(false);

            if (!updated)
            {
                calculationState.Status = CalculationStatus.Error;
            }

            return calculationState;
        }

        public async Task<CalculationState> MarkCalculationAsFinished(int integrationPointId, DocumentsStatistics statistics)
        {
            CalculationState calculationState = await GetCalculationStateValue(integrationPointId).ConfigureAwait(false);

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
            statistics.CalculatedOn = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);  // should we use 'UtcNow' here?
            calculationState.DocumentStatistics = statistics;

            await UpdateCalculationStateValue(integrationPointId, calculationState).ConfigureAwait(false);

            return calculationState;
        }

        public async Task<CalculationState> GetCalculationState(int integrationPointId)
        {
            return await GetCalculationStateValue(integrationPointId).ConfigureAwait(false);
        }

        public async Task<CalculationState> MarkCalculationAsCancelled(int integrationPointId)
        {
            CalculationState state = new CalculationState
            {
                Status = CalculationStatus.Canceled
            };

            await UpdateCalculationStateValue(integrationPointId, state).ConfigureAwait(false);
            return state;
        }

        private async Task<CalculationState> GetCalculationStateValue(int integrationPointId)
        {
            CalculationState result;
            try
            {
                QueryRequest queryRequest = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = Guid.Parse(ObjectTypeGuids.IntegrationPoint)
                    },
                    Fields = new FieldRef[] { new FieldRef { Guid = IntegrationPointFieldGuids.CalculationStateGuid } },
                    Condition = $"'Artifact ID' == {integrationPointId}"
                };

                ResultSet<RelativityObject> queryResult = await _relativityObjectManager.QueryAsync(queryRequest, 0, 1).ConfigureAwait(false);
                object fieldValue = queryResult?.Items.Single().FieldValues?.Single().Value;
                if (fieldValue == null)
                {
                    result = new CalculationState
                    {
                        Status = CalculationStatus.New
                    };
                }
                else
                {
                    string calculationStateValue = fieldValue.ToString();
                    result = JsonConvert.DeserializeObject<CalculationState>(calculationStateValue);
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

        private async Task<bool> UpdateCalculationStateValue(int integrationPointId, CalculationState currentState)
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

                return await _relativityObjectManager.UpdateAsync(integrationPointId, fieldValues, ExecutionIdentity.System).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not set calculation state for Integration Point {integrationPointId}", integrationPointId);
                return false;
            }
        }
    }
}
