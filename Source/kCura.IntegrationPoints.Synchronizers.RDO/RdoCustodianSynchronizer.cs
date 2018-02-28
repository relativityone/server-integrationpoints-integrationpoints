using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Custodian;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.Client;
using kCura.Utility.Extensions;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoCustodianSynchronizer : RdoSynchronizer
	{
		private const string LDAPMapFullNameFieldName = "CustomFullName";

		private readonly IAPILog _logger;
		private List<Artifact> _allRdoFields;

		private int _artifactTypeId;

		private IDictionary<string, string> _custodianManagerMap;
		public RdoCustodianSynchronizer(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IImportJobFactory jobFactory, IHelper helper)
			: base(fieldQuery, factory, jobFactory, helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoCustodianSynchronizer>();
		}

		public ITaskJobSubmitter TaskJobSubmitter { get; set; }

		public string FirstNameSourceFieldId { get; set; }
		public string LastNameSourceFieldId { get; set; }
		public string ManagerSourceFieldId { get; set; }
		public string UniqueIDSourceFieldId { get; set; }
		public bool HandleManagerLink { get; set; }
		public bool HandleFullNamePopulation { get; set; }

		private List<Artifact> GetAllRdoFields(ImportSettings settings)
		{
			if ((_artifactTypeId != settings.ArtifactTypeId) || (_allRdoFields == null))
			{
				_allRdoFields = GetRelativityFields(settings);
				_artifactTypeId = settings.ArtifactTypeId;
			}
			return _allRdoFields;
		}

		public override IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			LogRetrievingFields();
			var relativityFields = GetAllRdoFields(GetSettings(providerConfiguration.Configuration));
			var fields = base.GetFields(providerConfiguration);
			var fieldLookup = relativityFields.ToDictionary(x => x.ArtifactID.ToString(), x => x);

			foreach (var fieldEntry in fields)
			{
				if (!fieldLookup.ContainsKey(fieldEntry.FieldIdentifier))
				{
					yield return fieldEntry;
				}
				var artifact = fieldLookup[fieldEntry.FieldIdentifier];
				fieldEntry.IsIdentifier = IsField(artifact, Guid.Parse(CustodianFieldGuids.UniqueID));
				if (IsField(artifact, Guid.Parse(CustodianFieldGuids.FullName)))
				{
					continue;
				}
				var isRequired = IsField(artifact, Guid.Parse(CustodianFieldGuids.FirstName)) ||
								IsField(artifact, Guid.Parse(CustodianFieldGuids.LastName));

				fieldEntry.IsRequired = isRequired;
				yield return fieldEntry;
			}
		}

		protected override Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			try
			{
				_logger.LogDebug("Custodian field mapping process started...");
				var allRDOFields = GetAllRdoFields(settings);

				LoadFirstNameFieldId(fieldMap, allRDOFields);
				LoadLastNameFieldId(fieldMap, allRDOFields);
				LoadUniqueIdSourceField(fieldMap);

				Dictionary<string, int> importFieldMap = base.GetSyncDataImportFieldMap(fieldMap, settings);
				LoadFullNameField(allRDOFields, importFieldMap);
				_custodianManagerMap = new Dictionary<string, string>();

				LoadManagerFieldId(fieldMap, settings, allRDOFields);

				_logger.LogDebug("Custodian field mapping process finished");

				return importFieldMap;
			}
			catch (Exception ex)
			{
				string message = 
					$"Unable to get ImportMap for Custodians. \nFirstNameSourceFieldId: {FirstNameSourceFieldId}\nLastNameSourceFieldId {LastNameSourceFieldId}\nManagerSourceFieldId {ManagerSourceFieldId}\nUniqueIDSourceFieldId {UniqueIDSourceFieldId}\nHandleManagerLink {HandleManagerLink}\nHandleFullNamePopulation {HandleFullNamePopulation}";
				_logger.LogError(ex, @"Unable to get ImportMap for Custodians. \nFirstNameSourceFieldId: {FirstNameSourceFieldId}\nLastNameSourceFieldId {LastNameSourceFieldId}\nManagerSourceFieldId {ManagerSourceFieldId}\nUniqueIDSourceFieldId {UniqueIDSourceFieldId}\nHandleManagerLink {HandleManagerLink}\nHandleFullNamePopulation {HandleFullNamePopulation}", 
									 FirstNameSourceFieldId, LastNameSourceFieldId, ManagerSourceFieldId, UniqueIDSourceFieldId, HandleManagerLink, HandleFullNamePopulation);
				throw new IntegrationPointsException(message, ex)
				{
					ShouldAddToErrorsTab = true
				};
			}
		}

		private void LoadFirstNameFieldId(IEnumerable<FieldMap> fieldMap, List<Artifact> allRDOFields)
		{
			int firstNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.FirstName))).Select(x => x.ArtifactID).FirstOrDefault();
			if (fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(firstNameFieldId.ToString())))
			{
				FirstNameSourceFieldId =
					fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(firstNameFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
			}
		}

		private void LoadLastNameFieldId(IEnumerable<FieldMap> fieldMap, List<Artifact> allRDOFields)
		{
			int lastNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.LastName))).Select(x => x.ArtifactID).FirstOrDefault();
			if (fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(lastNameFieldId.ToString())))
			{
				LastNameSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(lastNameFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
			}
		}

		private void LoadUniqueIdSourceField(IEnumerable<FieldMap> fieldMap)
		{
			UniqueIDSourceFieldId = fieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).Select(x => x.SourceField.FieldIdentifier).First();
			_logger.LogDebug($"Custodian UniqueID source field identifier: {UniqueIDSourceFieldId}");
		}

		private void LoadManagerFieldId(IEnumerable<FieldMap> fieldMap, ImportSettings settings, List<Artifact> allRDOFields)
		{
			HandleManagerLink = false;

			int managerFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.Manager))).Select(x => x.ArtifactID).FirstOrDefault();
			_logger.LogDebug($"Destination workspace custodian rdo 'Manager' field artifact id: {managerFieldId}");

			if ((managerFieldId > 0) && fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(managerFieldId.ToString())))
			{
				ManagerSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(managerFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();

				_logger.LogDebug($"Destination workspace custodian rdo 'Manager' field artifact id: {managerFieldId} mapped to source provider field identifier: '{ManagerSourceFieldId}'");

				if (settings.CustodianManagerFieldContainsLink)
				{
					_logger.LogDebug("Identified custodian manager link setting...");
					HandleManagerLink = true;
				}
			}
		}

		private void LoadFullNameField(List<Artifact> allRDOFields, Dictionary<string, int> importFieldMap)
		{
			HandleFullNamePopulation = false;
			FieldEntry fullNameField =
				allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.FullName)))
					.Select(x => new FieldEntry { DisplayName = x.Name, FieldIdentifier = x.ArtifactID.ToString(), IsIdentifier = false })
					.FirstOrDefault();
			int fullNameFieldId = int.Parse(fullNameField.FieldIdentifier);


			if (!string.IsNullOrWhiteSpace(FirstNameSourceFieldId)
				&& !string.IsNullOrWhiteSpace(LastNameSourceFieldId)
				&& !importFieldMap.ContainsValue(fullNameFieldId))
			{
				importFieldMap.Add(LDAPMapFullNameFieldName, fullNameFieldId);
				HandleFullNamePopulation = true;
				_logger.LogDebug("Enabling custodian 'Full Name' auto population process");
			}
		}

		protected override Dictionary<string, object> GenerateImportRow(IDictionary<FieldEntry, object> row, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			LogGeneratingImportRow();
			var importRow = base.GenerateImportRow(row, fieldMap, settings);
			ProcessManagerReference(importRow);
			if (HandleFullNamePopulation && !importRow.ContainsKey(LDAPMapFullNameFieldName))
			{
				string firstName = (string) importRow[FirstNameSourceFieldId];
				string lastName = (string) importRow[LastNameSourceFieldId];
				string fullName = GenerateFullName(lastName, firstName);
				if (!string.IsNullOrWhiteSpace(fullName))
				{
					importRow.Add(LDAPMapFullNameFieldName, fullName);
				}
				else
				{
					//if no Full Name, do not insert record
					importRow = null;
					GenerateImportRowError(row, fieldMap, "Custodian is missing firstname and lastname. Record will be skipped.");
				}
			}
			return importRow;
		}

		private void GenerateImportRowError(IDictionary<FieldEntry, object> row, IEnumerable<FieldMap> fieldMap, string errorMessage)
		{
			string rowId = string.Empty;

			FieldMap idMap = fieldMap?.FirstOrDefault(map => map.FieldMapType == FieldMapTypeEnum.Identifier);
			if (idMap != null)
			{
				rowId = row[idMap.SourceField] as string ?? string.Empty;
				RaiseDocumentErrorEvent(rowId, errorMessage);
			}

			_logger.LogError("There was a problem with record: {rowId}.{errorMessage}",rowId, errorMessage);
		}

		protected override void FinalizeSyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			try
			{
				base.FinalizeSyncData(data, fieldMap, settings);

				if ((TaskJobSubmitter == null) || !_custodianManagerMap.Any())
				{
					LogMissingArguments();
					return;
				}
				_logger.LogDebug("Creating new Job for Manager refrence import");

				var jobParameters = new
				{
					CustodianManagerMap = _custodianManagerMap,
					CustodianManagerFieldMap = new List<FieldMap>
					{
						new FieldMap
						{
							SourceField =
								new FieldEntry
								{
									DisplayName = "CustodianIdentifier",
									FieldIdentifier = fieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).Select(x => x.SourceField.FieldIdentifier).First()
								},
							DestinationField = new FieldEntry {DisplayName = "ManagerIdentidier", FieldIdentifier = "distinguishedname"},
							FieldMapType = FieldMapTypeEnum.Identifier
						}
					},
					ManagerFieldIdIsBinary = false,
					ManagerFieldMap = fieldMap
				};

				LogSubmitingJob(jobParameters);
				TaskJobSubmitter.SubmitJob(jobParameters);

			}
			catch (Exception ex)
			{
				string message =
					$"Error occured while finalizing Custodian synchronization";
				_logger.LogError(ex, message);
				throw new IntegrationPointsException(message, ex)
				{
					ShouldAddToErrorsTab = true
				};
			}
		}

		public static string GenerateFullName(string lastName, string firstName)
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

		public void ProcessManagerReference(IDictionary<string, object> importRow)
		{
			if (HandleManagerLink)
			{
				string custodianUniqueIdentifier = (string) importRow[UniqueIDSourceFieldId];
				string managerReferenceLink = (string) importRow[ManagerSourceFieldId];
				LogProcessingManagerReference(managerReferenceLink, custodianUniqueIdentifier);
				if (!string.IsNullOrWhiteSpace(managerReferenceLink))
				{
					if (!string.IsNullOrWhiteSpace(custodianUniqueIdentifier) &&
					    !_custodianManagerMap.ContainsKey(custodianUniqueIdentifier))
					{
						_logger.LogDebug(
							$"Add Manager Ref Link: '{managerReferenceLink}' for custodian unique id field value: '{custodianUniqueIdentifier}'");
						_custodianManagerMap.Add(custodianUniqueIdentifier, managerReferenceLink);
					}

					importRow[ManagerSourceFieldId] = null;
				}
			}
		}

		public bool IsField(Artifact artifact, Guid fieldGuid)
		{
			return artifact.ArtifactGuids.Contains(fieldGuid);
		}

		#region Logging

		private void LogRetrievingFields()
		{
			_logger.LogInformation("Attempting to retrieve fields.");
		}

		private void LogGeneratingImportRow()
		{
			_logger.LogVerbose("Generating import row.");
		}

		private void LogSubmitingJob(object job)
		{
			_logger.LogDebug($"Attempting to submit job {JsonConvert.SerializeObject(job)}");
		}

		private void LogMissingArguments()
		{
			_logger.LogInformation("TaskJobSubmitter or no custodian manager found during sync data finalization.");
		}

		private void LogProcessingManagerReference(string mgrRefLinkId, string custodianUniqueIdentifier)
		{
			_logger.LogDebug($"Processing manager reference for custodian: {custodianUniqueIdentifier}, Manager Ref Link Id: '{mgrRefLinkId ?? "<empty>"}'");
		}

		#endregion
	}
}