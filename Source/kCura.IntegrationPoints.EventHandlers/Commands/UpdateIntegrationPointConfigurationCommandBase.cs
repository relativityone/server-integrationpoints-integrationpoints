using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Core.Extensions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.DTO;
using Relativity.Services.Objects.Exceptions;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;
using System.Runtime.CompilerServices;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public abstract class UpdateIntegrationPointConfigurationCommandBase : IEHCommand
    {
        private const string _REQUEST_ENTITY_TOO_LARGE_EXCEPTION = "Request Entity Too Large";

        private readonly IRelativityObjectManager _relativityObjectManager;

        protected readonly IAPILog _log;

        protected readonly IEHHelper _helper;

        protected abstract IList<string> FieldsNamesForUpdate { get; }

        protected abstract string SourceProviderGuid { get; }

        protected UpdateIntegrationPointConfigurationCommandBase(IEHHelper helper, IRelativityObjectManager relativityObjectManager)
        {
            _helper = helper;
            _relativityObjectManager = relativityObjectManager;
            _log = _helper.GetLoggerFactory().GetLogger().ForContext<UpdateIntegrationPointConfigurationCommandBase>();
        }
        public virtual void Execute()
        {
            SourceProvider sourceProvider = GetSourceProvider();
            if(sourceProvider == null)
            {
                _log.LogInformation("SourceProvider for Guid {provderGuid} does not exist. No Integration Points have been updated", SourceProviderGuid);
                return;
            }

            _log.LogInformation("Update IntegrationPoint Configuration Command for Provider {provderGuid} has been started.", SourceProviderGuid);

            QueryRequest query = GetIntegrationPointsBySourceProviderQuery(sourceProvider.ArtifactId);
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

                    MassUpdateIntegrationPoints(results);

                    start += results.Count;
                }
                while (results.Any());

                if(start == 0)
                {
                    _log.LogInformation("No Integration Points for SourceProvider {provderGuid} has been found.", SourceProviderGuid);
                }
            }
        }

        private QueryRequest GetIntegrationPointsBySourceProviderQuery(int sourceProviderId)
        {
            return new QueryRequest()
            {
                ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.IntegrationPointGuid },
                Condition = $"'{IntegrationPointFields.SourceProvider}' == {sourceProviderId}",
                Fields = FieldsNamesForUpdate.Select(x => new FieldRef { Name = x })
            };
        }

        private SourceProvider GetSourceProvider()
        {
            QueryRequest query = new QueryRequest()
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{SourceProviderGuid}'"
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

        private void MassUpdateIntegrationPoints(IList<RelativityObjectSlimDto> values)
        {
            try
            {
                _log.LogInformation("Trying to update {count} Integration Points", values.Count);

                if (!values.Any())
                {
                    return;
                }

                using (IObjectManager proxy = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
                {
                    MassUpdatePerObjectsRequest updateRequest = GetIntegrationPointsUpdateRequest(values);
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
                    throw new CommandExecutionException("Integration Point is too large for update request", ex);
                }

                foreach (var val in values.SplitList(batchSize))
                {
                    MassUpdateIntegrationPoints(val);
                }

                return;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Exception occurred during mass-updating Integration Points in {type}", this.GetType().Name);
                throw;
            }
        }

        private MassUpdatePerObjectsRequest GetIntegrationPointsUpdateRequest(IList<RelativityObjectSlimDto> values)
        {
            return new MassUpdatePerObjectsRequest()
            {
                Fields = FieldsNamesForUpdate.Select(x => new FieldRef { Name = x }).ToList(),
                ObjectValues = values.Select(x => new ObjectRefValuesPair()
                {
                    Object = new RelativityObjectRef { ArtifactID = x.ArtifactID },
                    Values = x.FieldValues.Values.ToList()
                }).ToList()
            };
        }

        protected abstract RelativityObjectSlimDto UpdateFields(RelativityObjectSlimDto values);
    }
}