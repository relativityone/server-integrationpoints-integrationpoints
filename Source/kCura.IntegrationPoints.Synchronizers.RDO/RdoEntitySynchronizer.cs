﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoEntitySynchronizer : RdoSynchronizer
	{
		private List<Artifact> _allRdoFields;
		private int _artifactTypeIdForAllRdoFields;
		private IDictionary<string, string> _entityManagerMap;

		private const string _LDAP_MAP_FULL_NAME_FIELD_NAME = "CustomFullName";

		private readonly IAPILog _logger;

		public RdoEntitySynchronizer(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IImportJobFactory jobFactory, IHelper helper)
			: base(fieldQuery, factory, jobFactory, helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoEntitySynchronizer>();
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
			List<Artifact> relativityFields = GetAllRdoFields(GetSettings(providerConfiguration.Configuration));
			IEnumerable<FieldEntry> fields = base.GetFields(providerConfiguration);
			Dictionary<string, Artifact> fieldLookup = relativityFields.ToDictionary(x => x.ArtifactID.ToString(), x => x);

			foreach (FieldEntry fieldEntry in fields)
			{
				if (!fieldLookup.ContainsKey(fieldEntry.FieldIdentifier))
				{
					yield return fieldEntry;
				}

				Artifact artifact = fieldLookup[fieldEntry.FieldIdentifier];
				fieldEntry.IsIdentifier = IsField(artifact, Guid.Parse(EntityFieldGuids.UniqueID));

				if (IsField(artifact, Guid.Parse(EntityFieldGuids.FullName)))
				{
					continue;
				}

				bool isRequired = IsField(artifact, Guid.Parse(EntityFieldGuids.FirstName)) ||
								IsField(artifact, Guid.Parse(EntityFieldGuids.LastName));

				fieldEntry.IsRequired = isRequired;
				yield return fieldEntry;
			}
		}

		protected override Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			try
			{
				_logger.LogDebug("Entity field mapping process started...");
				List<Artifact> allRdoFields = GetAllRdoFields(settings);

				LoadFirstNameFieldId(fieldMap, allRdoFields);
				LoadLastNameFieldId(fieldMap, allRdoFields);
				LoadUniqueIdSourceField(fieldMap);

				Dictionary<string, int> importFieldMap = base.GetSyncDataImportFieldMap(fieldMap, settings);
				LoadFullNameField(allRdoFields, importFieldMap);
				_entityManagerMap = new Dictionary<string, string>();

				LoadManagerFieldId(fieldMap, settings, allRdoFields);

				_logger.LogDebug("Entity field mapping process finished");

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

		private List<Artifact> GetAllRdoFields(ImportSettings settings)
		{
			if ((_artifactTypeIdForAllRdoFields != settings.ArtifactTypeId) || (_allRdoFields == null))
			{
				_allRdoFields = GetRelativityFields(settings);
				_artifactTypeIdForAllRdoFields = settings.ArtifactTypeId;
			}
			return _allRdoFields;
		}

		private void LoadFirstNameFieldId(IEnumerable<FieldMap> fieldMap, List<Artifact> allRDOFields)
		{
			int firstNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(EntityFieldGuids.FirstName))).Select(x => x.ArtifactID).FirstOrDefault();
			if (fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(firstNameFieldId.ToString())))
			{
				FirstNameSourceFieldId =
					fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(firstNameFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
			}
		}

		private void LoadLastNameFieldId(IEnumerable<FieldMap> fieldMap, List<Artifact> allRDOFields)
		{
			int lastNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(EntityFieldGuids.LastName))).Select(x => x.ArtifactID).FirstOrDefault();
			if (fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(lastNameFieldId.ToString())))
			{
				LastNameSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(lastNameFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
			}
		}

		private void LoadUniqueIdSourceField(IEnumerable<FieldMap> fieldMap)
		{
			UniqueIDSourceFieldId = fieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).Select(x => x.SourceField.FieldIdentifier).First();
			_logger.LogDebug($"Entity UniqueID source field identifier: {UniqueIDSourceFieldId}");
		}

		private void LoadManagerFieldId(IEnumerable<FieldMap> fieldMap, ImportSettings settings, List<Artifact> allRDOFields)
		{
			HandleManagerLink = false;

			int managerFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(EntityFieldGuids.Manager))).Select(x => x.ArtifactID).FirstOrDefault();
			_logger.LogDebug($"Destination workspace entity rdo 'Manager' field artifact id: {managerFieldId}");

			if ((managerFieldId > 0) && fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(managerFieldId.ToString())))
			{
				ManagerSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(managerFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();

				_logger.LogDebug($"Destination workspace entity rdo 'Manager' field artifact id: {managerFieldId} mapped to source provider field identifier: '{ManagerSourceFieldId}'");

				if (settings.EntityManagerFieldContainsLink)
				{
					_logger.LogDebug("Identified entity manager link setting...");
					HandleManagerLink = true;
				}
			}
		}

		private void LoadFullNameField(List<Artifact> allRDOFields, Dictionary<string, int> importFieldMap)
		{
			HandleFullNamePopulation = false;
			FieldEntry fullNameField =
				allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(EntityFieldGuids.FullName)))
					.Select(x => new FieldEntry { DisplayName = x.Name, FieldIdentifier = x.ArtifactID.ToString(), IsIdentifier = false })
					.FirstOrDefault();
			int fullNameFieldId = int.Parse(fullNameField.FieldIdentifier);


			if (!string.IsNullOrWhiteSpace(FirstNameSourceFieldId)
				&& !string.IsNullOrWhiteSpace(LastNameSourceFieldId)
				&& !importFieldMap.ContainsValue(fullNameFieldId))
			{
				importFieldMap.Add(_LDAP_MAP_FULL_NAME_FIELD_NAME, fullNameFieldId);
				HandleFullNamePopulation = true;
				_logger.LogDebug("Enabling entity 'Full Name' auto population process");
			}
		}

		protected override Dictionary<string, object> GenerateImportRow(IDictionary<FieldEntry, object> row, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			LogGeneratingImportRow();
			var importRow = base.GenerateImportRow(row, fieldMap, settings);
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
					//if no Full Name, do not insert record
					importRow = null;
					GenerateImportRowError(row, fieldMap, "Entity is missing firstname and lastname. Record will be skipped.");
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

			_logger.LogError("There was a problem with record: {rowId}.{errorMessage}", rowId, errorMessage);
		}

		protected override void FinalizeSyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			try
			{
				base.FinalizeSyncData(data, fieldMap, settings);
				SubmitLinkManagersJob(fieldMap);
			}
			catch (Exception ex)
			{
				string message =
					$"Error occured while finalizing Entity synchronization";
				_logger.LogError(ex, message);
				throw new IntegrationPointsException(message, ex)
				{
					ShouldAddToErrorsTab = true
				};
			}
		}

		private void SubmitLinkManagersJob(IEnumerable<FieldMap> fieldMap)
		{
			if (TaskJobSubmitter == null || !_entityManagerMap.Any())
			{
				LogMissingArguments();
				return;
			}

			_logger.LogDebug("Creating new Job for Manager reference import");

			var jobParameters = new
			{
				EntityManagerMap = _entityManagerMap,
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
						!_entityManagerMap.ContainsKey(uniqueIdentifier))
					{
						_logger.LogDebug(
							$"Add Manager Ref Link: '{managerReferenceLink}' for entity unique id field value: '{uniqueIdentifier}'");
						_entityManagerMap.Add(uniqueIdentifier, managerReferenceLink);
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
			_logger.LogInformation("TaskJobSubmitter or no entity manager found during sync data finalization.");
		}

		private void LogProcessingManagerReference(string mgrRefLinkId, string entityUniqueIdentifier)
		{
			_logger.LogDebug($"Processing manager reference for entity: {entityUniqueIdentifier}, Manager Ref Link Id: '{mgrRefLinkId ?? "<empty>"}'");
		}

		#endregion
	}
}