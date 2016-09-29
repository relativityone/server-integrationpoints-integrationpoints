using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.Custodian;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Artifact = kCura.Relativity.Client.Artifact;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
	[TestFixture]
	public class RdoCustodianSynchronizerTests
	{

		#region GetFields

		private string _settings;

		public static ImportApiFactory GetMockAPI(RelativityFieldQuery query)
		{
			var import = NSubstitute.Substitute.For<Relativity.ImportAPI.IExtendedImportAPI>();
			var result = query.GetFieldsForRdo(0);
			var list = new List<kCura.Relativity.ImportAPI.Data.Field>();
			var mi = typeof(Relativity.ImportAPI.Data.Field).GetProperty("ArtifactID").GetSetMethod(true);
			foreach (var r in result)
			{
				var f = new Relativity.ImportAPI.Data.Field();
				mi.Invoke(f, new object[] { r.ArtifactID });
				list.Add(f);
			}

			import.GetWorkspaceFields(Arg.Any<int>(), Arg.Any<int>()).Returns(list);
			var mock = NSubstitute.Substitute.For<ImportApiFactory>();
			mock.GetImportAPI(Arg.Any<ImportSettings>()).Returns(import);
			return mock;
		}

		[OneTimeSetUp]
		public void Setup()
		{
			_settings = JsonConvert.SerializeObject(new ImportSettings());
		}

		[Test]
		public void GetFields_FieldsContainsFirstName_MakesFieldRequired()
		{
			//ARRANGE
			var fieldQuery = NSubstitute.Substitute.For<RelativityFieldQuery>(NSubstitute.Substitute.For<IRSAPIClient>());
			var artifacts = new List<Artifact>();
			artifacts.Add(new Artifact
			{
				ArtifactID = 1,
				ArtifactGuids = new List<Guid> { Guid.Empty },
				Name = string.Empty
			});

			artifacts.Add(new Artifact
			{
				ArtifactID = 2,
				ArtifactGuids = new List<Guid> { Guid.Parse(CustodianFieldGuids.FirstName) },
				Name = "Test1"
			});

			fieldQuery.GetFieldsForRdo(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTests.ChangeWebAPIPath(new RdoCustodianSynchronizer(fieldQuery, GetMockAPI(fieldQuery)));
			var fields = sync.GetFields(_settings);


			//ASSERT
			var field = fields.First(x => x.DisplayName.Equals("Test1"));
			Assert.AreEqual(true, field.IsRequired);
		}


		[Test]
		public void GetFields_FieldsContainsLastName_MakesFieldRequired()
		{
			//ARRANGE
			var fieldQuery = NSubstitute.Substitute.For<RelativityFieldQuery>(NSubstitute.Substitute.For<IRSAPIClient>());
			var artifacts = new List<Artifact>();
			artifacts.Add(new Artifact
			{
				ArtifactID = 1,
				ArtifactGuids = new List<Guid> { Guid.Empty },
				Name = string.Empty
			});

			artifacts.Add(new Artifact
			{
				ArtifactID = 2,
				ArtifactGuids = new List<Guid> { Guid.Parse(CustodianFieldGuids.LastName) },
				Name = "Test1"
			});

			fieldQuery.GetFieldsForRdo(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTests.ChangeWebAPIPath(new RdoCustodianSynchronizer(fieldQuery, GetMockAPI(fieldQuery)));
			var fields = sync.GetFields(_settings);


			//ASSERT
			var field = fields.First(x => x.DisplayName.Equals("Test1"));
			Assert.AreEqual(true, field.IsRequired);
		}

		[Test]
		public void GetFields_FieldsContainsUniqueID_OnlyUniqueIDSetForIdentifier()
		{
			//ARRANGE
			var fieldQuery = NSubstitute.Substitute.For<RelativityFieldQuery>(NSubstitute.Substitute.For<IRSAPIClient>());
			var artifacts = new List<Artifact>();
			artifacts.Add(new Artifact
			{
				ArtifactID = 1,
				ArtifactGuids = new List<Guid> { Guid.Empty },
				Name = string.Empty
			});

			artifacts.Add(new Artifact
			{
				ArtifactID = 2,
				ArtifactGuids = new List<Guid> { Guid.Parse(CustodianFieldGuids.UniqueID) },
				Name = "Test1"
			});

			fieldQuery.GetFieldsForRdo(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTests.ChangeWebAPIPath(new RdoCustodianSynchronizer(fieldQuery, GetMockAPI(fieldQuery)));
			var fields = sync.GetFields(_settings).ToList();


			//ASSERT
			var idCount = fields.Count(x => x.IsIdentifier);
			Assert.AreEqual(1, idCount);
			var field = fields.Single(x => x.DisplayName.Equals("Test1"));
			Assert.AreEqual(true, field.IsIdentifier);
		}

		#endregion

	}
}