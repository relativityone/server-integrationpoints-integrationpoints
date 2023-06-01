using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class UpdateDestinationConfigurationTaggingSettingsCommand : IEHCommand
    {
        private const string _REQUEST_ENTITY_TOO_LARGE_EXCEPTION = "Request Entity Too Large";

        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IAPILog _log;
        private readonly IEHHelper _helper;

        private readonly string _sourceProviderGuid = Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY;
        private readonly Guid _integrationPointGuid = ObjectTypeGuids.IntegrationPointGuid;
        private readonly Guid _integrationPointProfileGuid = ObjectTypeGuids.IntegrationPointProfileGuid;

        private List<string> _fieldsNamesForUpdate => new List<string>
        {
            IntegrationPointFields.DestinationConfiguration
        };

        public UpdateDestinationConfigurationTaggingSettingsCommand(IEHHelper helper, IRelativityObjectManager relativityObjectManager)
        {
            _helper = helper;
            _relativityObjectManager = relativityObjectManager;
            _log = _helper.GetLoggerFactory().GetLogger().ForContext<UpdateDestinationConfigurationTaggingSettingsCommand>();
        }

        public void Execute()
        {
            SourceProvider sourceProvider = GetSourceProvider();
            if (sourceProvider == null)
            {
                _log.LogInformation("SourceProvider for Guid {providerGuid} does not exist. No RDOs have been updated", _sourceProviderGuid);
                return;
            }

            ExecuteInternal(sourceProvider, _integrationPointGuid);
            ExecuteInternal(sourceProvider, _integrationPointProfileGuid);
        }

        private void ExecuteInternal(SourceProvider sourceProvider, Guid objectTypeGuid)
        {
            _log.LogInformation("Update object of type: {type} for Provider {provderGuid} has been started.", objectTypeGuid.ToString(), _sourceProviderGuid);

            QueryRequest query = GetQueryRequest(sourceProvider.ArtifactId, objectTypeGuid);
            using (IExportQueryResult exportResult = _relativityObjectManager.QueryWithExportAsync(query, 0, ExecutionIdentity.System).Result)
            {
                string[] fieldNames = exportResult.ExportResult.FieldData.Select(f => f.Name).ToArray();
                IList<RelativityObjectSlimDto> results;
                int start = 0;
                do
                {
                    results = exportResult.GetNextBlockAsync(start).Result
                        .Select(x => x.ToRelativityObjectSlimDto(fieldNames))
                        .Select(x => UpdateFieldsWithValidation(x))
                        .Where(x => x != null).ToList();

                    MassUpdateObjects(results, objectTypeGuid.ToString());

                    start += results.Count;
                }
                while (results.Any());

                if (start == 0)
                {
                    _log.LogInformation("No objects of type: {type} for SourceProvider {provderGuid} has been found.", objectTypeGuid.ToString(), _sourceProviderGuid);
                }
            }
        }

        private RelativityObjectSlimDto UpdateFields(RelativityObjectSlimDto values)
        {
            return default;
        }

        private QueryRequest GetQueryRequest(int sourceProviderId, Guid objectTypeGuid)
        {
            return new QueryRequest()
            {
                ObjectType = new ObjectTypeRef { Guid = objectTypeGuid },
                Condition = $"'{IntegrationPointFields.SourceProvider}' == {sourceProviderId}",
                Fields = _fieldsNamesForUpdate.Select(x => new FieldRef { Name = x })
            };
        }

        private SourceProvider GetSourceProvider()
        {
            QueryRequest query = new QueryRequest()
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{_sourceProviderGuid}'"
            };

            return _relativityObjectManager.Query<SourceProvider>(query, ExecutionIdentity.System).FirstOrDefault();
        }

        private RelativityObjectSlimDto UpdateFieldsWithValidation(RelativityObjectSlimDto values)
        {
            IList<string> refValues = values.FieldValues.Keys.ToList();

            RelativityObjectSlimDto valuesToUpdate = UpdateFields(values);
            if (valuesToUpdate == null)
            {
                return null;
            }

            if (!refValues.SequenceEqual(valuesToUpdate.FieldValues.Keys))
            {
                throw new CommandExecutionException(
                    "Fields after update and retrieved fields does not match.");
            }

            return valuesToUpdate;
        }

        private void MassUpdateObjects(IList<RelativityObjectSlimDto> values, string objectTypeGuid)
        {
            try
            {
                _log.LogInformation("Trying to update {count} objects of type: {type}", values.Count, objectTypeGuid);

                if (!values.Any())
                {
                    return;
                }

                using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
                {
                    MassUpdatePerObjectsRequest updateRequest = GetMassUpdateRequest(values);
                    MassUpdateResult result = proxy.UpdateAsync(_helper.GetActiveCaseID(), updateRequest).Result;

                    _log.LogInformation("Update Status - Success: {status} Message: {message} TotalObjectsUpdate: {count} ",
                        result.Success, result.Message, result.TotalObjectsUpdated);
                }
            }
            catch (ServiceException ex) when (ex.Message.Contains(_REQUEST_ENTITY_TOO_LARGE_EXCEPTION))
            {
                const double numOfBatches = 2;
                int batchSize = (int)Math.Ceiling(values.Count() / numOfBatches);
                if (batchSize == values.Count())
                {
                    throw new CommandExecutionException($"Object of type {objectTypeGuid} is too large for update request", ex);
                }

                foreach (var val in values.SplitList(batchSize))
                {
                    MassUpdateObjects(val, objectTypeGuid);
                }

                return;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Exception occurred during mass-updating object of type: {objectType} in {type}", objectTypeGuid, this.GetType().Name);
                throw;
            }
        }

        private MassUpdatePerObjectsRequest GetMassUpdateRequest(IList<RelativityObjectSlimDto> values)
        {
            return new MassUpdatePerObjectsRequest()
            {
                Fields = _fieldsNamesForUpdate.Select(x => new FieldRef { Name = x }).ToList(),
                ObjectValues = values.Select(x => new ObjectRefValuesPair()
                {
                    Object = new RelativityObjectRef { ArtifactID = x.ArtifactID },
                    Values = x.FieldValues.Values.ToList()
                }).ToList()
            };
        }
    }
}
