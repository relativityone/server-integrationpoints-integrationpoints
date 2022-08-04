using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Utils;
using Newtonsoft.Json;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
    [DataSourceProvider(Domain.Constants.RELATIVITY_PROVIDER_GUID)]
    public class DocumentTransferProvider : IDataSourceProvider, IEmailBodyData
    {
        private readonly IImportApiFacade _importApiFacade;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IAPILog _logger;

        public DocumentTransferProvider(IImportApiFacade importApiFacade, IRepositoryFactory repositoryFactory, IAPILog logger)
        {
            _importApiFacade = importApiFacade;
            _repositoryFactory = repositoryFactory;
            _logger = logger.ForContext<DocumentTransferProvider>();
        }

        public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
        {
            LogRetrievingFields();
            var settings = JsonConvert.DeserializeObject<DocumentTransferSettings>(providerConfiguration.Configuration);
            ArtifactDTO[] fields = GetRelativityFields(settings.SourceWorkspaceArtifactId, Convert.ToInt32(ArtifactType.Document));
            IEnumerable<FieldEntry> fieldEntries = ParseFields(fields);

            return fieldEntries;
        }

        private static class Fields
        {
            internal static readonly string Name = "Name";
            internal static readonly string Choices = "Choices";
            internal static readonly string ObjectTypeArtifactTypeId = "Object Type Artifact Type ID";
            internal static readonly string FieldType = "Field Type";
            internal static readonly string FieldTypeId = "Field Type ID";
            internal static readonly string FieldTypeName = "Field Type Name";
            internal static readonly string IsIdentifier = "Is Identifier";
        }

        private ArtifactDTO[] GetRelativityFields(int sourceWorkspaceId, int rdoTypeId)
        {
            _logger.LogInformation("Relativity Fields retrieval process started for workspace id: {sourceWorkspaceId}", sourceWorkspaceId);
            var ignoreFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
                Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
                DocumentFields.RelativityDestinationCase,
                DocumentFields.JobHistory
            };

            _logger.LogInformation("Retrieving fields from Import API");
            HashSet<int> mappableArtifactIds =
                _importApiFacade.GetMappableArtifactIdsExcludeFields(sourceWorkspaceId, rdoTypeId, ignoreFields);

            _logger.LogInformation("Retrieving fields from Filed Repository");
            IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(sourceWorkspaceId);

            ArtifactDTO[] fieldArtifacts = fieldQueryRepository.RetrieveFields(
                rdoTypeId,
                new HashSet<string>(new[]
                {
                    Fields.Name,
                    Fields.Choices,
                    Fields.ObjectTypeArtifactTypeId,
                    Fields.FieldType,
                    Fields.FieldTypeId,
                    Fields.IsIdentifier,
                    Fields.FieldTypeName,
                }));

            // Contains is 0(1) https://msdn.microsoft.com/en-us/library/kw5aaea4.aspx

            // TODO: Why we need to call Import API and RsAPI to check teh fields are correct???
            ArtifactDTO[] filteredList = fieldArtifacts.Where(x => mappableArtifactIds.Contains(x.ArtifactId)).ToArray();

            _logger.LogInformation("Relativity Fields retrieval process completed for workspace id: {sourceWorkspaceId}",
                sourceWorkspaceId, filteredList);
            return filteredList;
        }

        private IEnumerable<FieldEntry> ParseFields(ArtifactDTO[] fieldArtifacts)
        {
            foreach (ArtifactDTO fieldArtifact in fieldArtifacts)
            {
                string fieldName = string.Empty;
                string fieldType = string.Empty;
                var isIdentifierFieldValue = 0;

                foreach (ArtifactFieldDTO field in fieldArtifact.Fields)
                {
                    if (field.Name == Fields.Name)
                    {
                        fieldName = field.Value as string;
                    }
                    else if (field.Name == Fields.IsIdentifier)
                    {
                        try
                        {
                            isIdentifierFieldValue = Convert.ToInt32(field.Value);
                        }
                        catch(Exception ex)
                        {
                            LogReceivingParsedFieldsError(fieldArtifacts, ex);
                            // suppress error for invalid casts
                        }
                    }
                    else if (field.Name == Fields.FieldType)
                    {
                        fieldType = Convert.ToString(field.Value);
                    }
                }

                bool isIdentifier = isIdentifierFieldValue > 0;
                if (isIdentifier)
                {
                    fieldName += Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT;
                }

                yield return new FieldEntry()
                {
                    DisplayName = fieldName,
                    Type = fieldType,
                    FieldIdentifier = fieldArtifact.ArtifactId.ToString(),
                    IsIdentifier = isIdentifier,
                    IsRequired = isIdentifier
                };
            }
        }

        /// <summary>
        /// Gets all of the artifact ids that can be batched in reads
        /// </summary>
        /// <param name="identifier">The identifying field (Control Number)</param>
        /// <param name="providerConfiguration">Data source provider configuration</param>
        /// <returns>An IDataReader containing all of the saved search's document artifact ids</returns>
        public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
        {
            LogRetrievingBatchableIdsErrorWithDetails(identifier, new NotImplementedException());
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the RDO's who's artifact ids exist in the entryIds list
        /// (This method is called in batches of normally 1000 entryIds)
        /// </summary>
        /// <param name="fields">The fieldArtifacts the user mapped</param>
        /// <param name="entryIds">The artifact ids of the documents to copy (in string format)</param>
        /// <param name="providerConfiguration">Data source provider configuration</param>
        /// <returns>An IDataReader that contains the Document RDO's for the entryIds</returns>
        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
        {
            LogRetrievingDataErrorWithDetails(entryIds, fields, new NotImplementedException());
            throw new NotImplementedException();
        }

        public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
        {
            LogReceivingEmailBodyData(fields);
            DocumentTransferSettings settings = GetSettings(options);

            WorkspaceDTO sourceWorkspace = GetWorkspace(settings.SourceWorkspaceArtifactId);

            var emailBody = new StringBuilder();
            if (sourceWorkspace != null)
            {
                emailBody.AppendLine(string.Empty);
                emailBody.Append(
                    $"Source Workspace: {WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(sourceWorkspace.Name, sourceWorkspace.ArtifactId)}");
            }
            return emailBody.ToString();
        }

        protected virtual DocumentTransferSettings GetSettings(string options)
        {
            return JsonConvert.DeserializeObject<DocumentTransferSettings>(options);
        }

        protected virtual WorkspaceDTO GetWorkspace(int workspaceArtifactIds)
        {
            IWorkspaceRepository workspaceRepository = _repositoryFactory.GetWorkspaceRepository();
            return workspaceRepository.Retrieve(workspaceArtifactIds);
        }

        #region Logging

        private void LogReceivingEmailBodyData(IEnumerable<FieldEntry> fields)
        {
            List<string> fieldIdentifiers = fields?.Select(x => x.FieldIdentifier).ToList() ?? new List<string>();
            _logger.LogInformation("Attempting to get email body data in Document Transfer Provider for fields {fields}.", string.Join(", ", fieldIdentifiers));
        }

        private void LogRetrievingFields()
        {
            _logger.LogInformation("Attempting to get fields in Document Transfer Provider.");
        }

        private void LogReceivingParsedFieldsError(IEnumerable<ArtifactDTO> fieldArtifacts, Exception ex)
        {
            List<int> items = fieldArtifacts?.Select(x => x.ArtifactId).ToList() ?? new List<int>();
            _logger.LogError(ex, "Failed to retrieve parsed fields in Document Transfer Provider (with {fieldArtifacts}).", string.Join(", ", items));
        }

        private void LogRetrievingBatchableIdsErrorWithDetails(FieldEntry identifier, Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve batchable ids in Document Transfer Provider for field {FieldIdentifier}.", identifier.FieldIdentifier);
        }

        private void LogRetrievingDataErrorWithDetails(IEnumerable<string> entryIds, IEnumerable<FieldEntry> fields, Exception ex)
        {
            List<string> fieldIdentifiers = fields?.Select(x => x.FieldIdentifier).ToList() ?? new List<string>();
            _logger.LogError(ex, "Failed to retrieve data in Document Transfer Provider for ids {Ids} and fields {fields}.",
                string.Join(",", entryIds), string.Join(",", fieldIdentifiers));
        }

        #endregion
    }
}