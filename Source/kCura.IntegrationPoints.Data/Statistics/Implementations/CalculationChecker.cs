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

        public async Task<CalculationState> MarkAsCalculating(int workspaceId, int integrationPointId)
        {
            CalculationState calculationState = new CalculationState
            {
                IsCalculating = true,
                HasErrors = false
            };

            bool updated = await UpdateCalculationStateValue(workspaceId, integrationPointId, calculationState).ConfigureAwait(false);

            if (!updated)
            {
                calculationState.IsCalculating = false;
                calculationState.HasErrors = true;
            }

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
                    HasErrors = true // int this case we should always have existing CalculationState field content!
                };
            }

            if (!calculationState.IsCalculating)
            {
                _logger.LogError("Could not finish calculation for Integration Point {integrationPointId} as calculation is not in progress", integrationPointId);
                calculationState.HasErrors = true;
                return calculationState;
            }

            calculationState.IsCalculating = false;
            statistics.CalculatedOn = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);  // should we use 'UtcNow' here?
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
                    HasErrors = true // something went wrong with reading RDO - otherwise we should get newly created CalculationState object
                };
            }

            return calculationState;
        }

        private async Task<CalculationState> GetCalculationStateValue(int workspaceId, int integrationPointId)
        {
            CalculationState result = null;
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
                        IsCalculating = false,
                        HasErrors = false
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
