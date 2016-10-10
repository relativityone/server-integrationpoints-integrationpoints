using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.Tests.Unit;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using FieldType = kCura.IntegrationPoints.Contracts.Models.FieldType;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Integration
{
	[TestFixture]
	[Ignore("This Fixture is for whatever reason excluded from building (when added it doesn't compile). It needs to be refactored or removed.")]
	public class RdoSynchronizerTest
	{
		[Test]
		public void FieldQueryTest()
		{
			//ARRANGE
			var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials())
			{
				APIOptions = { WorkspaceID = 1025517 }
			};
			var rdoQuery = new RSAPIRdoQuery(client);
			var rdo = new RdoSynchronizerBase(new RelativityFieldQuery(client), null);
			//ASSERT

			rdo.GetFields(JsonConvert.SerializeObject(new ImportSettings { ArtifactTypeId = 1000043 }));

		}

		[Test]
		public void ImportTest()
		{
			int CaseArtifactId = 1015527;
			int FieldID_UniqueID = 1038814;
			int FieldID_FullName = 1038502;
			int FieldID_Department = 1038799;

			var client = new RSAPIClient(new Uri("net.pipe://localhost/Relativity.Services"), new IntegratedAuthCredentials())
			{
				APIOptions = { WorkspaceID = CaseArtifactId }
			};
			var rdoQuery = new RSAPIRdoQuery(client);
			var fq = new RelativityFieldQuery(client);
			var rdo = new RdoSynchronizerBase(fq, RDOCustodianSynchronizerTests.GetMockAPI(fq));
			ImportSettings settings = new ImportSettings();

			settings.WebServiceURL = "http://localhost/RelativityWebAPI/";
			settings.ArtifactTypeId = 1000044; //Custodian
			settings.CaseArtifactId = CaseArtifactId;
			settings.ImportOverwriteMode = ImportOverwriteModeEnum.Append;
			settings.IdentityFieldId = FieldID_UniqueID;
			//settings.ParentObjectIdSourceFieldName= "parent";

			IEnumerable<FieldMap> map = new List<FieldMap>()
			{
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = "myname",FieldIdentifier = "myname",FieldType = FieldType.String},
					DestinationField	= new FieldEntry(){DisplayName = "",FieldIdentifier = FieldID_FullName.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = "dept",FieldIdentifier = "dept",FieldType = FieldType.String},
					DestinationField	= new FieldEntry(){DisplayName = "",FieldIdentifier = FieldID_Department.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.None
				},
				new FieldMap()
				{
					SourceField = new FieldEntry(){DisplayName = "guid",FieldIdentifier = "guid",FieldType = FieldType.String},
					DestinationField	= new FieldEntry(){DisplayName = "",FieldIdentifier = FieldID_UniqueID.ToString(),FieldType = FieldType.String},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};

			List<Dictionary<FieldEntry, object>> sourceFields = new List<Dictionary<FieldEntry, object>>();

			sourceFields.Add(new Dictionary<FieldEntry, object>()
			{
				{new FieldEntry(){DisplayName = "guid",FieldIdentifier = "guid",FieldType = FieldType.String},Guid.Parse("6703F851-C653-40E0-B249-AB4A7C879E6B")},
				{new FieldEntry(){DisplayName = "myname",FieldIdentifier = "myname",FieldType = FieldType.String},"Art"},
				{new FieldEntry(){DisplayName = "dept",FieldIdentifier = "dept",FieldType = FieldType.String},"DEV"}
			});
			sourceFields.Add(new Dictionary<FieldEntry, object>()
			{
				{new FieldEntry(){DisplayName = "guid",FieldIdentifier = "guid",FieldType = FieldType.String},Guid.Parse("7703F851-C653-40E0-B249-AB4A7C879E6B")},
				{new FieldEntry(){DisplayName = "myname",FieldIdentifier = "myname",FieldType = FieldType.String},"Chad"},
				{new FieldEntry(){DisplayName = "dept",FieldIdentifier = "dept",FieldType = FieldType.String},"IT"}
			});
			sourceFields.Add(new Dictionary<FieldEntry, object>()
			{
				{new FieldEntry(){DisplayName = "guid",FieldIdentifier = "guid",FieldType = FieldType.String},Guid.Parse("8703F851-C653-40E0-B249-AB4A7C879E6B")},
				{new FieldEntry(){DisplayName = "myname",FieldIdentifier = "myname",FieldType = FieldType.String},"New"},
				{new FieldEntry(){DisplayName = "dept",FieldIdentifier = "dept",FieldType = FieldType.String},"HR"}
			});
			rdo.SyncData(sourceFields, map, JsonConvert.SerializeObject(settings));
		}
	}
}
