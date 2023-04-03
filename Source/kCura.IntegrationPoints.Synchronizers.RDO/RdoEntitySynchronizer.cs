using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.Entity;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class RdoEntitySynchronizer : RdoSynchronizer
    {
        private List<RelativityObject> _allRdoFields;
        private int _artifactTypeIdForAllRdoFields;
        private OrderedDictionary _entityManagerMap;
        private const string _LDAP_MAP_FULL_NAME_FIELD_NAME = "CustomFullName";
        private readonly IAPILog _logger;
        private readonly IEntityManagerLinksSanitizer _entityManagerLinksSanitizer;

        public RdoEntitySynchronizer(
            IRelativityFieldQuery fieldQuery,
            IImportApiFactory factory,
            IImportJobFactory jobFactory,
            IHelper helper,
            IEntityManagerLinksSanitizer entityManagerLinksSanitizer,
            IDiagnosticLog diagnosticLog,
            IConfig config,
            ISerializer serializer)
            : base(fieldQuery, factory, jobFactory, helper, diagnosticLog, config, serializer)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoEntitySynchronizer>();
            _entityManagerLinksSanitizer = entityManagerLinksSanitizer;
            OnDocumentError += RemoveFailedItemFromEntityManagerMap;
        }

        public ITaskJobSubmitter TaskJobSubmitter { get; set; }

        private string FirstNameSourceFieldId { get; set; }

        private string LastNameSourceFieldId { get; set; }

        private string ManagerSourceFieldId { get; set; }

        private string UniqueIDSourceFieldId { get; set; }

        private bool HandleManagerLink { get; set; }

        private bool HandleFullNamePopulation { get; set; }

        public override IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
        {
            LogRetrievingFields();
            List<RelativityObject> relativityFields = GetAllRdoFields(Serializer.Deserialize<DestinationConfiguration>(providerConfiguration.Configuration));
            IEnumerable<FieldEntry> fields = base.GetFields(providerConfiguration);
            Dictionary<string, RelativityObject> fieldLookup = relativityFields.ToDictionary(x => x.ArtifactID.ToString(), x => x);

            foreach (FieldEntry fieldEntry in fields)
            {
                if (!fieldLookup.ContainsKey(fieldEntry.FieldIdentifier))
                {
                    yield return fieldEntry;
                }

                RelativityObject artifact = fieldLookup[fieldEntry.FieldIdentifier];
                fieldEntry.IsIdentifier = IsField(artifact, Guid.Parse(EntityFieldGuids.UniqueID));
                
                bool isRequired = IsField(artifact, Guid.Parse(EntityFieldGuids.FirstName)) ||
                                IsField(artifact, Guid.Parse(EntityFieldGuids.LastName));

                fieldEntry.IsRequired = isRequired;
                yield return fieldEntry;
            }
        }

        protected override Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, DestinationConfiguration destinationConfiguration)
        {
            try
            {
                _logger.LogInformation("Entity field mapping process started...");
                List<RelativityObject> allRdoFields = GetAllRdoFields(destinationConfiguration);

                LoadFirstNameFieldId(fieldMap, allRdoFields);
                LoadLastNameFieldId(fieldMap, allRdoFields);
                LoadUniqueIdSourceField(fieldMap);

                Dictionary<string, int> importFieldMap = base.GetSyncDataImportFieldMap(fieldMap, destinationConfiguration);
                LoadFullNameField(allRdoFields, importFieldMap);
                _entityManagerMap = new OrderedDictionary();

                LoadManagerFieldId(fieldMap, destinationConfiguration, allRdoFields);

                _logger.LogInformation("Entity field mapping process finished");

                return importFieldMap;
            }
            catch (Exception ex)
            {
                string message =
                    $"Unable to get ImportMap for Entity. \nFirstNameSourceFieldId: {FirstNameSourceFieldId}\nLastNameSourceFieldId {LastNameSourceFieldId}\nManagerSourceFieldId {ManagerSourceFieldId}\nUniqueIDSourceFieldId {UniqueIDSourceFieldId}\nHandleManagerLink {HandleManagerLink}\nHandleFullNamePopulation {HandleFullNamePopulation}";
                _logger.LogError(ex, @"Unable to get ImportMap for Entities. \nFirstNameSourceFieldId: {FirstNameSourceFieldId}\nLastNameSourceFieldId {LastNameSourceFieldId}\nManagerSourceFieldId {ManagerSourceFieldId}\nUniqueIDSourceFieldId {UniqueIDSourceFieldId}\nHandleManagerLink {HandleManagerLink}\nHandleFullNamePopulation {HandleFullNamePopulation}",
                                     FirstNameSourceFieldId, LastNameSourceFieldId, ManagerSourceFieldId, UniqueIDSourceFieldId, HandleManagerLink, HandleFullNamePopulation);
                throw new IntegrationPointsException(message, ex)
                {
                    ShouldAddToErrorsTab = true
                };
            }
        }

        private List<RelativityObject> GetAllRdoFields(DestinationConfiguration destinationConfiguration)
        {
            if ((_artifactTypeIdForAllRdoFields != destinationConfiguration.ArtifactTypeId) || (_allRdoFields == null))
            {
                _allRdoFields = GetRelativityFields(destinationConfiguration);
                _artifactTypeIdForAllRdoFields = destinationConfiguration.ArtifactTypeId;
            }
            return _allRdoFields;
        }

        private void LoadFirstNameFieldId(IEnumerable<FieldMap> fieldMap, List<RelativityObject> allRDOFields)
        {
            int firstNameFieldId = allRDOFields.Where(x => x.Guids.Contains(new Guid(EntityFieldGuids.FirstName))).Select(x => x.ArtifactID).FirstOrDefault();
            if (fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(firstNameFieldId.ToString())))
            {
                FirstNameSourceFieldId =
                    fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(firstNameFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
            }
        }

        private void LoadLastNameFieldId(IEnumerable<FieldMap> fieldMap, List<RelativityObject> allRDOFields)
        {
            int lastNameFieldId = allRDOFields.Where(x => x.Guids.Contains(new Guid(EntityFieldGuids.LastName))).Select(x => x.ArtifactID).FirstOrDefault();
            if (fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(lastNameFieldId.ToString())))
            {
                LastNameSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(lastNameFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
            }
        }

        private void LoadUniqueIdSourceField(IEnumerable<FieldMap> fieldMap)
        {
            UniqueIDSourceFieldId = fieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).Select(x => x.SourceField.FieldIdentifier).First();
            _logger.LogInformation("Entity UniqueID source field identifier: {UniqueIDSourceFieldId}", UniqueIDSourceFieldId);
        }

        private void LoadManagerFieldId(IEnumerable<FieldMap> fieldMap, DestinationConfiguration destinationConfiguration, List<RelativityObject> allRDOFields)
        {
            HandleManagerLink = false;

            int managerFieldId = allRDOFields.Where(x => x.Guids.Contains(new Guid(EntityFieldGuids.Manager))).Select(x => x.ArtifactID).FirstOrDefault();
            _logger.LogInformation($"Destination workspace entity rdo 'Manager' field artifact id: {managerFieldId}");

            if ((managerFieldId > 0) && fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(managerFieldId.ToString())))
            {
                ManagerSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(managerFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();

                _logger.LogInformation("Destination workspace entity rdo 'Manager' field artifact id: {managerFieldId} mapped to source provider field identifier: '{ManagerSourceFieldId}'", managerFieldId, ManagerSourceFieldId);

                if (destinationConfiguration.EntityManagerFieldContainsLink)
                {
                    _logger.LogInformation("Identified entity manager link setting...");
                    HandleManagerLink = true;
                }
            }
        }

        private void LoadFullNameField(List<RelativityObject> allRDOFields, Dictionary<string, int> importFieldMap)
        {
            HandleFullNamePopulation = false;
            FieldEntry fullNameField = allRDOFields
                .Where(x => x.Guids.Contains(new Guid(EntityFieldGuids.FullName)))
                .Select(x => new FieldEntry
                {
                    DisplayName = x.Name,
                    FieldIdentifier = x.ArtifactID.ToString(),
                    IsIdentifier = false
                })
                .FirstOrDefault();
            int fullNameFieldId = int.Parse(fullNameField.FieldIdentifier);

            if (!string.IsNullOrWhiteSpace(FirstNameSourceFieldId)
                && !string.IsNullOrWhiteSpace(LastNameSourceFieldId)
                && !importFieldMap.ContainsValue(fullNameFieldId))
            {
                importFieldMap.Add(_LDAP_MAP_FULL_NAME_FIELD_NAME, fullNameFieldId);
                HandleFullNamePopulation = true;
                _logger.LogInformation("Enabling entity 'Full Name' auto population process");
            }
        }

        protected override Dictionary<string, object> GenerateImportRow(IDictionary<FieldEntry, object> row, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
        {
            Dictionary<string, object> importRow = base.GenerateImportRow(row, fieldMap, settings);
            ProcessManagerReference(importRow);
            if (HandleFullNamePopulation && !importRow.ContainsKey(_LDAP_MAP_FULL_NAME_FIELD_NAME))
            {
                string firstName = (string)importRow[FirstNameSourceFieldId];
                string lastName = (string)importRow[LastNameSourceFieldId];
                string fullName = GenerateFullName(lastName, firstName);

                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    importRow.Add(_LDAP_MAP_FULL_NAME_FIELD_NAME, fullName);
                }
                else
                {
                    string sourceFieldId = importRow[UniqueIDSourceFieldId] as string ?? string.Empty;
                    GenerateImportRowError(sourceFieldId, "Entity is missing firstname and lastname. Record will be skipped.");
                    // if no Full Name, do not insert record
                    importRow = null;
                }
            }
            return importRow;
        }

        private void RemoveFailedItemFromEntityManagerMap(string row, string error)
        {
            _entityManagerMap.Remove(row);
        }

        private void GenerateImportRowError(string sourceFieldId, string errorMessage)
        {
            RaiseDocumentErrorEvent(sourceFieldId, errorMessage);
            _logger.LogWarning("There was a problem with record: {sanitizedSourceFieldId}. {errorMessage}", SanitizeString(sourceFieldId), errorMessage);
        }

        private string SanitizeString(string sensitiveString)
        {
            int lettersToShow = 2;
            // because how sensitive can be 4 letters word?
            if (sensitiveString.Length <= lettersToShow * 2)
            {
                return sensitiveString;
            }
            return sensitiveString.Substring(0, lettersToShow) + "..." + sensitiveString.Substring(sensitiveString.Length - lettersToShow);
        }

        protected override void FinalizeSyncData(IEnumerable<IDictionary<FieldEntry, object>> data,
            IEnumerable<FieldMap> fieldMap, ImportSettings settings, IJobStopManager jobStopManager)
        {
            try
            {
                base.FinalizeSyncData(data, fieldMap, settings, jobStopManager);
                SubmitLinkManagersJob(fieldMap);
            }
            catch (Exception ex)
            {
                string message =
                    "Error occured while finalizing Entity synchronization";
                _logger.LogError(ex, message);
                throw new IntegrationPointsException(message, ex)
                {
                    ShouldAddToErrorsTab = true
                };
            }
        }

        private void SubmitLinkManagersJob(IEnumerable<FieldMap> fieldMap)
        {
            if (TaskJobSubmitter == null || _entityManagerMap.Count == 0)
            {
                LogMissingArguments();
                return;
            }

            _logger.LogInformation("Creating new Job for Manager reference import");

            Dictionary<string, string> finalEntityManagerMap = GetEntityManagerMapForTransferredRecords(ImportService.TotalRowsProcessed);

            var jobParameters = new
            {
                EntityManagerMap = finalEntityManagerMap,
                EntityManagerFieldMap = new List<FieldMap>
                {
                    new FieldMap
                    {
                        SourceField =
                            new FieldEntry
                            {
                                DisplayName = "CustodianIdentifier",
                                FieldIdentifier = fieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).Select(x => x.SourceField.FieldIdentifier).First()
                            },
                        DestinationField = new FieldEntry {DisplayName = "ManagerIdentidier", FieldIdentifier = _entityManagerLinksSanitizer.ManagerLinksFieldIdentifier },
                        FieldMapType = FieldMapTypeEnum.Identifier
                    }
                },
                ManagerFieldIdIsBinary = false,
                ManagerFieldMap = fieldMap
            };

            LogSubmittingJob();
            TaskJobSubmitter.SubmitJob(jobParameters);
        }

        private Dictionary<string, string> GetEntityManagerMapForTransferredRecords(int transferredItemsCount)
        {
            int i = 0;
            Dictionary<string, string> finalEntityManagerMap = new Dictionary<string, string>();
            foreach (DictionaryEntry entry in _entityManagerMap)
            {
                if (i < transferredItemsCount)
                {
                    finalEntityManagerMap.Add(entry.Key.ToString(), entry.Value.ToString());
                    i++;
                }
            }

            return finalEntityManagerMap;
        }

        private static string GenerateFullName(string lastName, string firstName)
        {
            string fullName = string.Empty;
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                fullName = lastName;
            }
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    fullName += ", ";
                }
                fullName += firstName;
            }
            return fullName;
        }

        private void ProcessManagerReference(IDictionary<string, object> importRow)
        {
            if (HandleManagerLink)
            {
                string uniqueIdentifier = (string)importRow[UniqueIDSourceFieldId];
                string managerReferenceLink = (string)importRow[ManagerSourceFieldId];
                LogProcessingManagerReference(managerReferenceLink, uniqueIdentifier);
                if (!string.IsNullOrWhiteSpace(managerReferenceLink))
                {
                    if (!string.IsNullOrWhiteSpace(uniqueIdentifier) &&
                        !_entityManagerMap.Contains(uniqueIdentifier))
                    {
                        managerReferenceLink = _entityManagerLinksSanitizer.SanitizeManagerReferenceLink(managerReferenceLink);

                        _logger.LogInformation(
                            "Add Manager Ref Link: '{0}' for entity unique id field value: '{1}'", managerReferenceLink, uniqueIdentifier);
                        _entityManagerMap.Add(uniqueIdentifier, managerReferenceLink);
                    }

                    importRow[ManagerSourceFieldId] = null;
                }
            }
        }

        private bool IsField(RelativityObject artifact, Guid fieldGuid)
        {
            return artifact.Guids.Contains(fieldGuid);
        }

        #region Logging

        private void LogRetrievingFields()
        {
            _logger.LogInformation("Attempting to retrieve fields.");
        }

        private void LogSubmittingJob()
        {
            _logger.LogInformation("Attempting to submit job.");
        }

        private void LogMissingArguments()
        {
            _logger.LogInformation("TaskJobSubmitter or no entity manager found during sync data finalization.");
        }

        private void LogProcessingManagerReference(string mgrRefLinkId, string entityUniqueIdentifier)
        {
            mgrRefLinkId = mgrRefLinkId ?? "<empty>";
            _logger.LogInformation("Processing manager reference for entity: {0}, Manager Ref Link Id: '{1}'", entityUniqueIdentifier, mgrRefLinkId);
        }

        #endregion
    }
}
