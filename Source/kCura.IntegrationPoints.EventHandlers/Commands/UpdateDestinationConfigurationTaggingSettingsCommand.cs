using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using Newtonsoft.Json;
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
        private readonly string _destinationConfigurationFieldName = IntegrationPointFields.DestinationConfiguration;
        private readonly Guid _integrationPointGuid = ObjectTypeGuids.IntegrationPointGuid;
        private readonly Guid _integrationPointProfileGuid = ObjectTypeGuids.IntegrationPointProfileGuid;

        private List<string> _fieldsNamesForUpdate => new List<string>
        {
            _destinationConfigurationFieldName
        };

        public UpdateDestinationConfigurationTaggingSettingsCommand(IEHHelper helper, IRelativityObjectManager relativityObjectManager)
        {
            _helper = helper;
            _relativityObjectManager = relativityObjectManager;
            _log = _helper.GetLoggerFactory().GetLogger().ForContext<UpdateDestinationConfigurationTaggingSettingsCommand>();
        }

        public void Execute()
        {
            try
            {
                SourceProvider sourceProvider = GetSourceProvider();
                if (sourceProvider == null)
                {
                    _log.LogWarning("SourceProvider for Guid {providerGuid} does not exist. No RDOs have been updated", _sourceProviderGuid);
                    return;
                }

                ExecuteInternal(sourceProvider, _integrationPointGuid);
                ExecuteInternal(sourceProvider, _integrationPointProfileGuid);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error occurred during execution command of type {type}", this.GetType().Name);
            }
        }

        private void ExecuteInternal(SourceProvider sourceProvider, Guid objectTypeGuid)
        {
            QueryRequest query = GetMigrationCandidateQuery(sourceProvider.ArtifactId, objectTypeGuid);
            using (IExportQueryResult exportResult = _relativityObjectManager.QueryWithExportAsync(query, 0, ExecutionIdentity.System).Result)
            {
                string[] fieldNames = exportResult.ExportResult.FieldData.Select(f => f.Name).ToArray();
                IList<RelativityObjectSlimDto> block;
                int readItemsCount = 0;
                int updatedItemsCount = 0;
                int failedItemsCount = 0;
                do
                {
                    block = exportResult.GetNextBlockAsync(readItemsCount).Result
                        .Select(x => x.ToRelativityObjectSlimDto(fieldNames))
                        .ToList();

                    var updatedItems = block.Select(MigrateTaggingSettings)
                            .Where(x => x != null)
                            .ToList();

                    try
                    {
                        MassUpdateObjects(updatedItems, objectTypeGuid.ToString());
                        updatedItemsCount += updatedItems.Count;
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Unable to mass update block of objects during {command}", nameof(UpdateDestinationConfigurationTaggingSettingsCommand));
                        failedItemsCount += updatedItems.Count;
                    }

                    readItemsCount += block.Count;
                }
                while (block.Any());

                _log.LogWarning(
                    "Command {command} execution report for object type {objectTypeGuid}: total: {totalCount}, updated: {updatedCount}, failed: {failedCount}",
                    nameof(UpdateDestinationConfigurationTaggingSettingsCommand),
                    objectTypeGuid,
                    readItemsCount,
                    updatedItemsCount,
                    failedItemsCount);
            }
        }

        private RelativityObjectSlimDto MigrateTaggingSettings(RelativityObjectSlimDto migrationCandidate)
        {
            const string enableTaggingPropertyName = "EnableTagging";
            const string taggingOptionPropertyName = "TaggingOption";

            string destinationConfigurationString = migrationCandidate.FieldValues[_destinationConfigurationFieldName] as string;
            IDictionary<string, object> destinationConfigurationDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(destinationConfigurationString);

            if (destinationConfigurationDictionary.TryGetValue(enableTaggingPropertyName, out object enableTaggingValue))
            {
                destinationConfigurationDictionary.Remove(enableTaggingPropertyName);
                destinationConfigurationDictionary[taggingOptionPropertyName] = Convert.ToBoolean(enableTaggingValue.ToString())
                    ? "Enabled"
                    : "Disabled";

                string updatedDestinationConfiguration = JsonConvert.SerializeObject(destinationConfigurationDictionary, Formatting.None);
                migrationCandidate.FieldValues[_destinationConfigurationFieldName] = updatedDestinationConfiguration;
                return migrationCandidate;
            }

            return null; // null means there was no migration need
        }

        private QueryRequest GetMigrationCandidateQuery(int sourceProviderId, Guid objectTypeGuid)
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

        private void MassUpdateObjects(IList<RelativityObjectSlimDto> values, string objectTypeGuid)
        {
            if (values.Any() == false)
            {
                return;
            }

            try
            {
                using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
                {
                    MassUpdatePerObjectsRequest updateRequest = GetMassUpdateRequest(values);
                    MassUpdateResult result = proxy.UpdateAsync(_helper.GetActiveCaseID(), updateRequest).Result;
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
                ObjectValues = values
                    .Select(x => new ObjectRefValuesPair
                        {
                            Object = new RelativityObjectRef { ArtifactID = x.ArtifactID },
                            Values = x.FieldValues.Values.ToList()
                        })
                    .ToList()
            };
        }
    }
}
