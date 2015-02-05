using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RDOCustodianSynchronizer : RdoSynchronizer
	{
		private const string CustodianFieldGuid_FullName = @"57928ef5-f29d-4137-a215-3a9abf3e3f82";
		private const string CustodianFieldGuid_FirstName = @"34ee9d29-44bd-4fc5-8ff1-4335a826a07d";
		private const string CustodianFieldGuid_LastName = @"0b846e7a-6e05-4544-b5a8-ad78c49d0257";
		private const string LDAPMapFullNameFieldName = "CustomFullName";

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
				fieldEntry.IsIdentifier = fieldEntry.DisplayName.Equals("UniqueID");
				if (fieldEntry.DisplayName.Equals("firstname"))
				{
					continue;
				}
				yield return fieldEntry;
			}
		}

		protected override Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			var allRDOFields = GetAllRdoFields(settings.ArtifactTypeId);

			FieldEntry fullNameField = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuid_FullName))).Select(x => new FieldEntry() { DisplayName = x.Name, FieldIdentifier = x.ArtifactID.ToString(), IsIdentifier = false }).FirstOrDefault();

			Dictionary<string, int> importFieldMap = base.GetSyncDataImportFieldMap(fieldMap, settings);

			int fullNameFieldId = int.Parse(fullNameField.FieldIdentifier);
			if (!importFieldMap.ContainsValue(fullNameFieldId))
			{
				importFieldMap.Add(LDAPMapFullNameFieldName, fullNameFieldId);
			}

			int firstNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuid_FirstName))).Select(x => x.ArtifactID).FirstOrDefault();
			int lastNameFieldId = allRDOFields.Where(x => x.ArtifactGuids.Contains(new Guid(CustodianFieldGuid_LastName))).Select(x => x.ArtifactID).FirstOrDefault();
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
	}
}
