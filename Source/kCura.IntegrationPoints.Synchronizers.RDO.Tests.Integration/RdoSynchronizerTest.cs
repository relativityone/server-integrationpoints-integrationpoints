using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Assert = NUnit.Framework.Assert;
using Document = kCura.Relativity.Client.DTOs.Document;
using FieldType = kCura.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration
{
	[TestFixture]
	public class RdoSynchronizerTest : SourceProviderTemplate
	{
		private IRelativityFieldQuery _fieldQuery;
		private IImportApiFactory _factory;
		private IImportJobFactory _jobFactory;
		private IHelper _helper;

		private const string _INPUT_DATA_CONTROL_NUMBER = "guid";
		private const string _INPUT_DATA_EXTRACTED_TEXT = "extractedtext";
		private const string _INPUT_DATA_GROUP_ID = "groupid";
		private const string _WORKSPACE_CONTROL_NUMBER = "Control Number";
		private const string _WORKSPACE_EXTRACTED_TEXT = "Extracted Text";
		private const string _WORKSPACE_GROUP_ID = "Group Identifier";

		public RdoSynchronizerTest() : base($"RdoSynchronizerTest_{Utils.FormattedDateTimeNow}")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_fieldQuery = Container.Resolve<IRelativityFieldQuery>();
			_factory = Container.Resolve<IImportApiFactory>();
			_jobFactory = Container.Resolve<IImportJobFactory>();
			_helper = Container.Resolve<IHelper>();
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.SMOKE_TEST)]
		public void ItShouldReturnSourceWorkspaceFields()
		{
			//Arrange
			var rdoSynchronizer = new RdoSynchronizer(_fieldQuery, _factory, _jobFactory, _helper);
			ImportSettings importSettings = CreateDefaultImportSettings();
			string settings = JsonConvert.SerializeObject(importSettings);

			//Act
			List<FieldEntry> fields = rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(settings)).ToList();

			//Assert
			Assert.NotNull(fields);
			Assert.Greater(fields.Count, 0);
			Assert.NotNull(fields.FirstOrDefault(field => field.IsIdentifier));
			Assert.NotNull(fields.FirstOrDefault(field => field.IsRequired));
		}

		[Test]
		[Category(IntegrationPoint.Tests.Core.Constants.SMOKE_TEST)]
		public void ItShouldSyncDataToWorkspace()
		{
			//Arrange
			var rdoSynchronizer = new RdoSynchronizer(_fieldQuery, _factory, _jobFactory, _helper);

			ImportSettings importSettings = CreateDefaultImportSettings();
			List<FieldEntry> destinationFields = rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(JsonConvert.SerializeObject(importSettings))).ToList();
			FieldEntry fieldIdentifierEntry = destinationFields.FirstOrDefault(field => field.IsIdentifier);
			importSettings.ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly;
			if (fieldIdentifierEntry != null)
			{
				importSettings.IdentityFieldId = int.Parse(fieldIdentifierEntry.FieldIdentifier);
		    }
			
			string settings = JsonConvert.SerializeObject(importSettings);
			IEnumerable<FieldMap> sourceFields = CreateDefaultSourceFieldMap(rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(settings)).ToList());

			List<Dictionary<FieldEntry, object>> importData = CreateImportData();

			//Act
			rdoSynchronizer.SyncData(importData, sourceFields, settings);

			//Assert
			List<Result<Document>> documents = GetAllDocuments(WorkspaceArtifactId);
			Assert.AreEqual(importData.Count, documents.Count);
			VerifyDocumentsWithDefaultData(importData, documents);
		}

		#region "Helpers"

		private static void VerifyDocumentsWithDefaultData(List<Dictionary<FieldEntry, object>> dataList, List<Result<Document>> documents)
		{
			foreach (Dictionary<FieldEntry, object> data in dataList)
			{
				FieldEntry controlNumberKey = data.Keys.Single(x => x.ActualName == _INPUT_DATA_CONTROL_NUMBER);
				object expectedControlNumber = data[controlNumberKey];

				FieldEntry extractedTextKey = data.Keys.Single(x => x.ActualName == _INPUT_DATA_EXTRACTED_TEXT);
				object expectedExtractedText = data[extractedTextKey];

				Result<Document> docResult = documents.FirstOrDefault(x => x.Artifact.TextIdentifier == expectedControlNumber.ToString());
				Assert.NotNull(docResult);
				FieldValue docExtractedText = docResult.Artifact.Fields.First(x => x.Name == _WORKSPACE_EXTRACTED_TEXT);
				Assert.AreEqual(expectedExtractedText.ToString(), docExtractedText.Value.ToString());
			}
		}

		public List<Result<Document>> GetAllDocuments(int workspaceId)
		{
			using (var proxy = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				proxy.APIOptions.WorkspaceID = workspaceId;

				var requestedFields = new[] { _WORKSPACE_CONTROL_NUMBER, _WORKSPACE_EXTRACTED_TEXT, _WORKSPACE_GROUP_ID };
				List<FieldValue> fields = requestedFields.Select(x => new FieldValue(x)).ToList();
				var query = new Query<Document>
				{
					Fields = fields
				};

				QueryResultSet<Document> result = null;
				result = proxy.Repositories.Document.Query(query, 0);
				return result.Results;
			}
		}

		private static IEnumerable<FieldMap> CreateDefaultSourceFieldMap(List<FieldEntry> destinationFields)
		{
			if (destinationFields == null)
			{
				return null;
			}
			int fieldIdUniqueId = int.Parse(destinationFields.FirstOrDefault(field => field.IsIdentifier).FieldIdentifier);
			int fieldIdExtractedText = int.Parse(destinationFields.Single(field => field.ActualName == _WORKSPACE_EXTRACTED_TEXT).FieldIdentifier);
			int fieldIdGroupIdentifier = int.Parse(destinationFields.Single(field => field.ActualName == _WORKSPACE_GROUP_ID).FieldIdentifier);

			return new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = _INPUT_DATA_EXTRACTED_TEXT,FieldIdentifier = _INPUT_DATA_EXTRACTED_TEXT,FieldType = FieldType.String},
					DestinationField    = new FieldEntry(){DisplayName = "",FieldIdentifier = fieldIdExtractedText.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = _INPUT_DATA_GROUP_ID,FieldIdentifier = _INPUT_DATA_GROUP_ID,FieldType = FieldType.String},
					DestinationField    = new FieldEntry(){DisplayName = "",FieldIdentifier = fieldIdGroupIdentifier.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = _INPUT_DATA_CONTROL_NUMBER,FieldIdentifier = _INPUT_DATA_CONTROL_NUMBER,FieldType = FieldType.String},
					DestinationField    = new FieldEntry(){DisplayName = "",FieldIdentifier = fieldIdUniqueId.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};
		}

		private static List<Dictionary<FieldEntry, object>> CreateImportData()
		{
			var sourceFields = new List<Dictionary<FieldEntry, object>>
			{
				new Dictionary<FieldEntry, object>()
				{
					{
						new FieldEntry() {DisplayName = _INPUT_DATA_CONTROL_NUMBER, FieldIdentifier = _INPUT_DATA_CONTROL_NUMBER, FieldType = FieldType.String},
						Guid.Parse("6703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{new FieldEntry() {DisplayName = _INPUT_DATA_EXTRACTED_TEXT, FieldIdentifier = _INPUT_DATA_EXTRACTED_TEXT, FieldType = FieldType.String}, "Art"},
					{new FieldEntry() {DisplayName = _INPUT_DATA_GROUP_ID, FieldIdentifier = _INPUT_DATA_GROUP_ID, FieldType = FieldType.String}, "DEV"}
				},
				new Dictionary<FieldEntry, object>()
				{
					{
						new FieldEntry() {DisplayName = _INPUT_DATA_CONTROL_NUMBER, FieldIdentifier = _INPUT_DATA_CONTROL_NUMBER, FieldType = FieldType.String},
						Guid.Parse("7703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{new FieldEntry() {DisplayName = _INPUT_DATA_EXTRACTED_TEXT, FieldIdentifier = _INPUT_DATA_EXTRACTED_TEXT, FieldType = FieldType.String}, "Chad"},
					{new FieldEntry() {DisplayName = _INPUT_DATA_GROUP_ID, FieldIdentifier = _INPUT_DATA_GROUP_ID, FieldType = FieldType.String}, "IT"}
				},
				new Dictionary<FieldEntry, object>()
				{
					{
						new FieldEntry() {DisplayName = _INPUT_DATA_CONTROL_NUMBER, FieldIdentifier = _INPUT_DATA_CONTROL_NUMBER, FieldType = FieldType.String},
						Guid.Parse("8703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{new FieldEntry() {DisplayName = _INPUT_DATA_EXTRACTED_TEXT, FieldIdentifier = _INPUT_DATA_EXTRACTED_TEXT, FieldType = FieldType.String}, "New"},
					{new FieldEntry() {DisplayName = _INPUT_DATA_GROUP_ID, FieldIdentifier = _INPUT_DATA_GROUP_ID, FieldType = FieldType.String}, "HR"}
				}
			};

			return sourceFields;
		}

		private ImportSettings CreateDefaultImportSettings()
		{
			var importSettings = new ImportSettings
			{
				ArtifactTypeId = Convert.ToInt32(ArtifactType.Document),
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword,
				WebServiceURL = SharedVariables.RelativityWebApiUrl,
				CaseArtifactId = WorkspaceArtifactId
			};

			return importSettings;
		}

		#endregion
	}
}
