using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RDOCustodianSynchronizer : RdoSynchronizer
	{

		private const string LDAPMapFullNameFieldName = "CustomFullName";

		public static class CustodianFieldGuids
		{
			public const string FullName = @"57928ef5-f29d-4137-a215-3a9abf3e3f82";
			public const string BCCPeople = @"3f10b4e5-f7a6-4774-a102-660f36110310";
			public const string RelatedCustodians = @"abc6e98d-68fb-428a-bd42-b59f47f1d18a";
			public const string Respondents = @"6946d84b-7043-488b-b08c-4b41a4de1f8b";
			public const string Notes = @"08bc4e08-a955-4e87-b648-c0a33e40b7a4";
			public const string DocumentNumberingPrefix = @"bae96568-2b16-4707-8074-be267f205d9c";
			public const string CurrentTitle = @"21c26ce8-5833-4b29-ab86-93ca8fc90ab5";
			public const string Email = @"fd825796-2143-4817-9467-11589295cd04";
			public const string Department = @"a9363e45-2db8-4735-8ea4-9eeb950a6ffb";
			public const string Domain = @"a92c1f70-b11f-4c55-8165-0f93dcc1d308";
			public const string EmployeeNumber = @"168d66a9-b57e-4fe7-b2b3-7dc1c1cff20e";
			public const string EmployeeStatus = @"3ef3f876-5ab0-4a7e-b88e-68c12f032338";
			public const string EmploymentEndDate = @"70a33f5a-7076-4af5-a26c-9ce42801eaf9";
			public const string EmploymentStartDate = @"403e3208-4a92-4236-aeb8-15ea3d094b04";
			public const string FirstName = @"34ee9d29-44bd-4fc5-8ff1-4335a826a07d";
			public const string LastName = @"0b846e7a-6e05-4544-b5a8-ad78c49d0257";
			public const string Manager = @"80bd28d7-dcfb-42d8-bb85-39e4af0051d2";
			public const string PastDepartment = @"5bfa7942-588b-4113-b95b-87325cd97ea8";
			public const string PastManager = @"408a0b2b-b21b-4494-98af-1da7ec53a8fd";
			public const string PastTitle = @"921245ca-81ed-4011-af53-eca2e06bd004";
			public const string PhoneNumber = @"7de80974-5815-48e9-8faf-a51c1c66da0e";
			public const string RelativityLegalHoldJobID = @"e5a8e65a-9bd1-4828-86e9-02748f01eb32";
			public const string SecondaryEmail = @"31ded4e9-f37f-42b5-b222-f36b9da1417b";
			public const string UniqueID = @"3c5f8ef5-4ed9-40be-b404-1c70318b3563";
		}

		public RDOCustodianSynchronizer(RelativityFieldQuery fieldQuery, RelativityRdoQuery rdoQuery)
			: base(fieldQuery, rdoQuery)
		{
		}

		public string FirstNameSourceFieldId { get; set; }
		public string LastNameSourceFieldId { get; set; }


		private int _artifactTypeId = 0;
		private List<Artifact> _allRdoFields;
		private List<Artifact> GetAllRdoFields(int artifactTypeId)
		{
			if (_artifactTypeId != artifactTypeId || _allRdoFields == null)
			{
				_allRdoFields = FieldQuery.GetFieldsForRDO(artifactTypeId);
				_artifactTypeId = artifactTypeId;
			}
			return _allRdoFields;
		}

		public override IEnumerable<FieldEntry> GetFields(string options)
		{
			var relativityFields = GetAllRdoFields(GetSettings(options).ArtifactTypeId);
			var fields = ParseFields(relativityFields);
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
				var isRequired = (IsField(artifact, Guid.Parse(CustodianFieldGuids.FirstName)) ||
													IsField(artifact, Guid.Parse(CustodianFieldGuids.LastName)));

				fieldEntry.IsRequired = isRequired;
				yield return fieldEntry;
			}
		}

		protected override Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			var allRDOFields = GetAllRdoFields(settings.ArtifactTypeId);

			FieldEntry fullNameField = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.FullName))).Select(x => new FieldEntry() { DisplayName = x.Name, FieldIdentifier = x.ArtifactID.ToString(), IsIdentifier = false }).FirstOrDefault();

			Dictionary<string, int> importFieldMap = base.GetSyncDataImportFieldMap(fieldMap, settings);

			int fullNameFieldId = int.Parse(fullNameField.FieldIdentifier);
			if (!importFieldMap.ContainsValue(fullNameFieldId))
			{
				importFieldMap.Add(LDAPMapFullNameFieldName, fullNameFieldId);
			}

			int firstNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.FirstName))).Select(x => x.ArtifactID).FirstOrDefault();
			int lastNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuids.LastName))).Select(x => x.ArtifactID).FirstOrDefault();
			FirstNameSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier == firstNameFieldId.ToString()).Select(x => x.SourceField.FieldIdentifier).First();
			LastNameSourceFieldId = fieldMap.Where(x => x.DestinationField.FieldIdentifier == lastNameFieldId.ToString()).Select(x => x.SourceField.FieldIdentifier).First();

			return importFieldMap;
		}

		protected override Dictionary<string, object> GenerateImportRow(IDictionary<FieldEntry, object> row, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{

			Dictionary<string, int> importFieldMap = fieldMap.Where(x => x.FieldMapType != FieldMapTypeEnum.Parent)
				.ToDictionary(x => x.SourceField.FieldIdentifier, x => int.Parse(x.DestinationField.FieldIdentifier));

			var importRow = base.GenerateImportRow(row, fieldMap, settings);
			if (!importRow.ContainsKey(LDAPMapFullNameFieldName))
			{
				string firstName = FirstNameSourceFieldId;
				string lastName = LastNameSourceFieldId;
				string fullName = string.Empty;
				if (!string.IsNullOrWhiteSpace(lastName))
				{
					fullName = lastName;
				}
				if (!string.IsNullOrWhiteSpace(firstName))
				{
					if (!string.IsNullOrWhiteSpace(firstName)) fullName += ", ";
					fullName += firstName;
				}
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

		public bool IsField(Relativity.Client.Artifact artifact, Guid fieldGuid)
		{
			return artifact.ArtifactGuids.Contains(fieldGuid);
		}

	}
}
