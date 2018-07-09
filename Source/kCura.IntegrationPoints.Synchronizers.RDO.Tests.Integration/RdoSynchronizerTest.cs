using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

		public RdoSynchronizerTest() 
			: base($"RdoSynchronizerTest_{Utils.FormatedDateTimeNow}")
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
		[Ignore("TODO: Broken test needs to be fixed!", Until = "2018-07-18")]
		public void ItShouldSyncDataToWorkspace()
		{
			//Arange
			const int numberOfDocuments = 3;
			var rdoSynchronizer = new RdoSynchronizer(_fieldQuery, _factory, _jobFactory, _helper);
			
			ImportSettings importSettings = CreateDefaultImportSettings();
			List<FieldEntry> destinationFields = rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(JsonConvert.SerializeObject(importSettings))).ToList();
			FieldEntry fieldIdentifierEntry = destinationFields.FirstOrDefault(field => field.IsIdentifier);
			importSettings.ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly;
			if (fieldIdentifierEntry != null) { importSettings.IdentityFieldId = int.Parse(fieldIdentifierEntry.FieldIdentifier);}

			string settings = JsonConvert.SerializeObject(importSettings);
			
			IEnumerable<FieldMap> sourceFields = CreateDefaultSourceFieldMap(rdoSynchronizer.GetFields(new DataSourceProviderConfiguration(settings)).ToList());

			List<Dictionary<FieldEntry, object>> defaultData = CreateDefaultData();

			//Act
			rdoSynchronizer.SyncData(defaultData, sourceFields, settings);

			List<Result<Document>> documents = GetAllDocuments(WorkspaceArtifactId);

			//Assert
			Assert.AreEqual(numberOfDocuments, documents.Count);
			VerifyDocumentsWithDefaultData(defaultData, documents);
		}

		#region "Helpers"

		private static void VerifyDocumentsWithDefaultData(List<Dictionary<FieldEntry, object>> dataList, List<Result<Document>> documents)
		{
			foreach (Dictionary<FieldEntry, object> data in dataList)
			{
				object expectedControlNumber = data.Values.ElementAt(0);
				object expectedExtractedText = data.Values.ElementAt(2);
				Result<Document> docResult = documents.FirstOrDefault(x => x.Artifact.TextIdentifier == expectedControlNumber.ToString());
				Assert.NotNull(docResult);
				FieldValue docExtractedText = docResult.Artifact.Fields.First(x => x.Name == "Extracted Text");
				Assert.AreEqual(expectedExtractedText.ToString(), docExtractedText.Value.ToString());
			}
		}
		
		public List<Result<Document>> GetAllDocuments(int workspaceId)
		{
			using (var proxy = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				proxy.APIOptions.WorkspaceID = workspaceId;

				var requestedFields = new[] {"Control Number", "Extracted Text", "Group Identifier"};
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
			int fieldIdFullName = int.Parse(destinationFields.Where(field => !field.IsIdentifier).ElementAt(0).FieldIdentifier);
			int fieldIdDepartment = int.Parse(destinationFields.Where(field => !field.IsIdentifier).ElementAt(1).FieldIdentifier);

			return new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = "myname",FieldIdentifier = "myname",FieldType = FieldType.String},
					DestinationField    = new FieldEntry(){DisplayName = "",FieldIdentifier = fieldIdFullName.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = "dept",FieldIdentifier = "dept",FieldType = FieldType.String},
					DestinationField    = new FieldEntry(){DisplayName = "",FieldIdentifier = fieldIdDepartment.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = "guid",FieldIdentifier = "guid",FieldType = FieldType.String},
					DestinationField    = new FieldEntry(){DisplayName = "",FieldIdentifier = fieldIdUniqueId.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};
		}

		private static List<Dictionary<FieldEntry, object>> CreateDefaultData()
		{
			var sourceFields = new List<Dictionary<FieldEntry, object>>
			{
				new Dictionary<FieldEntry, object>()
				{
					{
						new FieldEntry() {DisplayName = "guid", FieldIdentifier = "guid", FieldType = FieldType.String},
						Guid.Parse("6703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{new FieldEntry() {DisplayName = "myname", FieldIdentifier = "myname", FieldType = FieldType.String}, "Art"},
					{new FieldEntry() {DisplayName = "dept", FieldIdentifier = "dept", FieldType = FieldType.String}, "DEV"}
				},
				new Dictionary<FieldEntry, object>()
				{
					{
						new FieldEntry() {DisplayName = "guid", FieldIdentifier = "guid", FieldType = FieldType.String},
						Guid.Parse("7703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{new FieldEntry() {DisplayName = "myname", FieldIdentifier = "myname", FieldType = FieldType.String}, "Chad"},
					{new FieldEntry() {DisplayName = "dept", FieldIdentifier = "dept", FieldType = FieldType.String}, "IT"}
				},
				new Dictionary<FieldEntry, object>()
				{
					{
						new FieldEntry() {DisplayName = "guid", FieldIdentifier = "guid", FieldType = FieldType.String},
						Guid.Parse("8703F851-C653-40E0-B249-AB4A7C879E6B")
					},
					{new FieldEntry() {DisplayName = "myname", FieldIdentifier = "myname", FieldType = FieldType.String}, "New"},
					{new FieldEntry() {DisplayName = "dept", FieldIdentifier = "dept", FieldType = FieldType.String}, "HR"}
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
