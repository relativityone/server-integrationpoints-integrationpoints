using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Logging
{
    internal class FieldMappingSummary : IFieldMappingSummary
    {
        private readonly IFieldConfiguration _configuration;
        private readonly IFieldManager _fieldManager;
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IAPILog _logger;

        public FieldMappingSummary(IFieldConfiguration configuration, IFieldManager fieldManager, ISourceServiceFactoryForUser serviceFactoryForUser, IAPILog logger)
        {
            _configuration = configuration;
            _fieldManager = fieldManager;
            _serviceFactoryForUser = serviceFactoryForUser;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> GetFieldsMappingSummaryAsync(CancellationToken token)
        {
            IList<FieldInfoDto> mappedFields = await _fieldManager.GetMappedFieldsAsync(token).ConfigureAwait(false);

            Task<Dictionary<string, RelativityObjectSlim>> sourceFieldsDetailsTask = GetFieldsDetailsAsync(
                _configuration.SourceWorkspaceArtifactId,
                mappedFields
                    .Where(x => x.RelativityDataType == RelativityDataType.LongText)
                    .Select(x => x.SourceFieldName)
                    .ToList(), _configuration.RdoArtifactTypeId, token);

            Task<Dictionary<string, RelativityObjectSlim>> destinationFieldsDetailsTask = GetFieldsDetailsAsync(
                _configuration.DestinationWorkspaceArtifactId,
                mappedFields
                    .Where(x => x.RelativityDataType == RelativityDataType.LongText)
                    .Select(x => x.DestinationFieldName)
                    .ToList(), _configuration.DestinationRdoArtifactTypeId, token);

            await Task.WhenAll(sourceFieldsDetailsTask, destinationFieldsDetailsTask).ConfigureAwait(false);
            Dictionary<string, object> fieldsMappingSummary = GetFieldsMappingSummary(mappedFields, sourceFieldsDetailsTask.Result, destinationFieldsDetailsTask.Result);
            return fieldsMappingSummary;
        }

        private async Task<Dictionary<string, RelativityObjectSlim>> GetFieldsDetailsAsync(
            int workspaceId,
            List<string> fieldNames, int rdoArtifactTypeId, CancellationToken token)
        {
            if (fieldNames == null || !fieldNames.Any())
            {
                return new Dictionary<string, RelativityObjectSlim>();
            }

            ICollection<string> requestedFieldNames = new HashSet<string>(fieldNames);
            IEnumerable<string> formattedFieldNames = requestedFieldNames.Select(KeplerQueryHelpers.EscapeForSingleQuotes).Select(f => $"'{f}'");
            string concatenatedFieldNames = string.Join(", ", formattedFieldNames);

            QueryRequest request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { Name = "Field" },
                Condition =
                    $"'Name' IN [{concatenatedFieldNames}] AND 'Object Type Artifact Type ID' == {rdoArtifactTypeId}",
                Fields = new[]
                {
                    new FieldRef { Name = "Name" },
                    new FieldRef { Name = "Enable Data Grid" }
                }
            };

            QueryResultSlim result;
            using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                try
                {
                    const int start = 0;
                    result = await objectManager
                        .QuerySlimAsync(workspaceId, request, start, requestedFieldNames.Count, token)
                        .ConfigureAwait(false);
                }
                catch (ServiceException ex)
                {
                    _logger.LogError(
                        ex,
                        "Service call failed while querying non document fields in workspace {workspaceArtifactId} for mapping details",
                        workspaceId);
                    throw new SyncKeplerException(
                        $"Service call failed while querying non document fields in workspace {workspaceId} for mapping details",
                        ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to query non document fields in workspace {workspaceArtifactId} for mapping details",
                        workspaceId);
                    throw new SyncKeplerException(
                        $"Failed to query non document fields in workspace {workspaceId} for mapping details", ex);
                }
            }

            return result.Objects.ToDictionary(x => x.Values[0].ToString(), x => x);
        }

        private Dictionary<string, object> GetFieldsMappingSummary(
            IList<FieldInfoDto> mappings,
            IDictionary<string, RelativityObjectSlim> sourceLongTextFieldsDetails,
            IDictionary<string, RelativityObjectSlim> destinationLongTextFieldsDetails)
        {
            const string keyFormat = "[{0}] <--> [{1}]";

            Dictionary<string, int> mappingSummary = mappings
                .GroupBy(x => x.RelativityDataType, x => x)
                .ToDictionary(x => x.Key.ToString(), x => x.Count());

            Dictionary<string, Dictionary<string, Dictionary<string, object>>> longTextFields = mappings
                .Where(x => x.RelativityDataType == RelativityDataType.LongText)
                .ToDictionary(x => string.Format(keyFormat, x.SourceFieldName, x.DestinationFieldName), x =>
                {
                    RelativityObjectSlim sourceField = sourceLongTextFieldsDetails[x.SourceFieldName];
                    RelativityObjectSlim destinationField = destinationLongTextFieldsDetails[x.DestinationFieldName];

                    return new Dictionary<string, Dictionary<string, object>>
                    {
                        {
                            "Source", new Dictionary<string, object>()
                            {
                                { "ArtifactId", sourceField.ArtifactID },
                                { "DataGridEnabled", sourceField.Values[1] }
                            }
                        },
                        {
                            "Destination", new Dictionary<string, object>()
                            {
                                { "ArtifactId", destinationField.ArtifactID },
                                { "DataGridEnabled", destinationField.Values[1] }
                            }
                        }
                    };
                });

            const string extractedTextFieldName = "Extracted Text";
            string extractedTextKey = string.Format(keyFormat, extractedTextFieldName, extractedTextFieldName);

            Dictionary<string, object> summary = new Dictionary<string, object>()
            {
                { "FieldMapping", mappingSummary },
                { "LongText", longTextFields.Where(x => x.Key != extractedTextKey).Select(x => x.Value).ToArray() }
            };

            if (_configuration.RdoArtifactTypeId == (int)ArtifactType.Document)
            {
                summary.Add("ExtractedText", longTextFields.TryGetValue(extractedTextKey, out var v) ? v : null);
            }

            return summary;
        }
    }
}
