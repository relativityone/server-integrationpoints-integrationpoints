using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class RdoSynchronizerTest : SourceProviderTemplate
	{
		private IRelativityFieldQuery _fieldQuery;
		private IImportApiFactory _factory;
		private IImportJobFactory _jobFactory;
		private IHelper _helper;

		private const string _INPUT_DATA_CONTROL_NUMBER = "guid";
		private const string _INPUT_DATA_EXTRACTED_TEXT = "extractedtext";
		private const string _WORKSPACE_EXTRACTED_TEXT = "Extracted Text";

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

		[IdentifiedTest("750ec1e6-7446-44bd-9dbd-641d04e5b8a4")]
		[SmokeTest]
		[Ignore("")]
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

		[IdentifiedTest("26241015-fc5b-4443-bdf1-1545fd40cdd9")]
		[SmokeTest]
		public async Task ItShouldSyncDataToWorkspace()
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
			List<RelativityObject> documents = await GetAllDocumentsAsync().ConfigureAwait(false);
			Assert.AreEqual(importData.Count, documents.Count);
			VerifyDocumentsWithDefaultData(importData, documents);
		}

		#region "Helpers"

		private static void VerifyDocumentsWithDefaultData(List<Dictionary<FieldEntry, object>> dataList, List<RelativityObject> documents)
		{
			foreach (Dictionary<FieldEntry, object> data in dataList)
			{
				FieldEntry controlNumberKey = data.Keys.Single(x => x.ActualName == _INPUT_DATA_CONTROL_NUMBER);
				object expectedControlNumber = data[controlNumberKey];

				FieldEntry extractedTextKey = data.Keys.Single(x => x.ActualName == _INPUT_DATA_EXTRACTED_TEXT);
				object expectedExtractedText = data[extractedTextKey];

				RelativityObject document = documents.FirstOrDefault(x => x.Name == expectedControlNumber.ToString());

				Assert.NotNull(document);

				object docExtractedTextValue = document.FieldValues.Single(x => x.Field.Name == _WORKSPACE_EXTRACTED_TEXT).Value;
				Assert.AreEqual(expectedExtractedText.ToString(), docExtractedTextValue.ToString());
			}
		}

		public async Task<List<RelativityObject>> GetAllDocumentsAsync()
		{
			using (var objectManager = Helper.CreateProxy<IObjectManager>())
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef {ArtifactTypeID = (int)ArtifactType.Document},
					Fields = new List<FieldRef>
					{
						new FieldRef {Name = _WORKSPACE_EXTRACTED_TEXT}
					},
					IncludeNameInQueryResult = true
				};

				QueryResult result = await objectManager.QueryAsync(WorkspaceArtifactId, request, 0, int.MaxValue)
					.ConfigureAwait(false);

				return result.Objects;
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

			return new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						DisplayName = _INPUT_DATA_EXTRACTED_TEXT,
						FieldIdentifier = _INPUT_DATA_EXTRACTED_TEXT,
						FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = "",
						FieldIdentifier = fieldIdExtractedText.ToString(),
						FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
					},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						DisplayName = _INPUT_DATA_CONTROL_NUMBER,
						FieldIdentifier = _INPUT_DATA_CONTROL_NUMBER,
						FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = "",
						FieldIdentifier = fieldIdUniqueId.ToString(),
						FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
					},
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
						new FieldEntry()
						{
							DisplayName = _INPUT_DATA_CONTROL_NUMBER,
							FieldIdentifier = _INPUT_DATA_CONTROL_NUMBER,
							FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
						},
						Guid.Parse("6703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{
						new FieldEntry()
						{
							DisplayName = _INPUT_DATA_EXTRACTED_TEXT,
							FieldIdentifier = _INPUT_DATA_EXTRACTED_TEXT,
							FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
						},
						"Art"
					}
				},
				new Dictionary<FieldEntry, object>()
				{
					{
						new FieldEntry()
						{
							DisplayName = _INPUT_DATA_CONTROL_NUMBER,
							FieldIdentifier = _INPUT_DATA_CONTROL_NUMBER,
							FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
						},
						Guid.Parse("7703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{
						new FieldEntry()
						{
							DisplayName = _INPUT_DATA_EXTRACTED_TEXT,
							FieldIdentifier = _INPUT_DATA_EXTRACTED_TEXT,
							FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
						},
						"Chad"
					}
				},
				new Dictionary<FieldEntry, object>()
				{
					{
						new FieldEntry()
						{
							DisplayName = _INPUT_DATA_CONTROL_NUMBER,
							FieldIdentifier = _INPUT_DATA_CONTROL_NUMBER,
							FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
						},
						Guid.Parse("8703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{
						new FieldEntry()
						{
							DisplayName = _INPUT_DATA_EXTRACTED_TEXT,
							FieldIdentifier = _INPUT_DATA_EXTRACTED_TEXT,
							FieldType = global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String
						},
						"New"
					}
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
