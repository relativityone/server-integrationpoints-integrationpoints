using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Custodian;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
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

		public RdoCustodianSynchronizer(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IHelper helper)
			: base(fieldQuery, factory, helper)
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

		public override IEnumerable<FieldEntry> GetFields(string options)
		{
			LogRetrievingFields();
			var relativityFields = GetAllRdoFields(GetSettings(options));
			var fields = base.GetFields(options);
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
			var allRDOFields = GetAllRdoFields(settings);

			FieldEntry fullNameField =
				allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.FullName)))
					.Select(x => new FieldEntry {DisplayName = x.Name, FieldIdentifier = x.ArtifactID.ToString(), IsIdentifier = false})
					.FirstOrDefault();

			Dictionary<string, int> importFieldMap = base.GetSyncDataImportFieldMap(fieldMap, settings);

			int firstNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.FirstName))).Select(x => x.ArtifactID).FirstOrDefault();
			int lastNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.LastName))).Select(x => x.ArtifactID).FirstOrDefault();
			int managerFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.Manager))).Select(x => x.ArtifactID).FirstOrDefault();
			if (fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(firstNameFieldId.ToString())))
			{
				FirstNameSourceFieldId =
					fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(firstNameFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
			}
			if (fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(lastNameFieldId.ToString())))
			{
				LastNameSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(lastNameFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
			}
			UniqueIDSourceFieldId = fieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).Select(x => x.SourceField.FieldIdentifier).First();

			HandleFullNamePopulation = false;
			int fullNameFieldId = int.Parse(fullNameField.FieldIdentifier);
			if (!string.IsNullOrWhiteSpace(FirstNameSourceFieldId)
				&& !string.IsNullOrWhiteSpace(LastNameSourceFieldId)
				&& !importFieldMap.ContainsValue(fullNameFieldId))
			{
				importFieldMap.Add(LDAPMapFullNameFieldName, fullNameFieldId);
				HandleFullNamePopulation = true;
			}

			HandleManagerLink = false;
			_custodianManagerMap = new Dictionary<string, string>();
			if ((managerFieldId > 0) && fieldMap.Any(x => x.DestinationField.FieldIdentifier.Equals(managerFieldId.ToString())))
			{
				ManagerSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier.Equals(managerFieldId.ToString())).Select(x => x.SourceField.FieldIdentifier).First();
				if (settings.CustodianManagerFieldContainsLink)
				{
					HandleManagerLink = true;
				}
			}

			return importFieldMap;
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
				}
			}
			return importRow;
		}

		protected override void FinalizeSyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			base.FinalizeSyncData(data, fieldMap, settings);

			if ((TaskJobSubmitter == null) || !_custodianManagerMap.Any())
			{
				LogMissingArguments();
				return;
			}

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
						DestinationField = new FieldEntry {DisplayName = "ManagerIdentidier", FieldIdentifier = "distinguishedName"},
						FieldMapType = FieldMapTypeEnum.Identifier
					}
				},
				ManagerFieldIdIsBinary = false,
				ManagerFieldMap = fieldMap
			};

			LogSubmitingJob();
			TaskJobSubmitter.SubmitJob(jobParameters);
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
			if (!HandleManagerLink)
			{
				return;
			}

			LogProcessingManagerReference();
			string managerReferenceLink = (string) importRow[ManagerSourceFieldId];
			if (!string.IsNullOrWhiteSpace(managerReferenceLink))
			{
				string custodianUniqueIdentifier = (string) importRow[UniqueIDSourceFieldId];
				if (!string.IsNullOrWhiteSpace(custodianUniqueIdentifier))
				{
					_custodianManagerMap.Add(custodianUniqueIdentifier, managerReferenceLink);
				}
				importRow[ManagerSourceFieldId] = null;
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

		private void LogSubmitingJob()
		{
			_logger.LogVerbose("Attempting to submit job.");
		}

		private void LogMissingArguments()
		{
			_logger.LogInformation("TaskJobSubmitter or no custodian manager found during sync data finalization.");
		}

		private void LogProcessingManagerReference()
		{
			_logger.LogVerbose("Processing manager reference.");
		}

		#endregion
	}
}