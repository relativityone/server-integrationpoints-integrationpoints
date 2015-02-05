using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using NUnit.Framework;
using NSubstitute;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Unit
{
	[TestFixture]
	public class RDOCustodianSynchronizerTests
	{

		#region GetFields

		private string _settings;

		[TestFixtureSetUp]
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
				ArtifactGuids = new List<Guid> { Guid.Parse(RDOCustodianSynchronizer.CustodianFieldGuids.FirstName) },
				Name = "Test1"
			});

			fieldQuery.GetFieldsForRDO(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTest.ChangeWebAPIPath(new RDOCustodianSynchronizer(fieldQuery, null));
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
				ArtifactGuids = new List<Guid> { Guid.Parse(RDOCustodianSynchronizer.CustodianFieldGuids.LastName) },
				Name = "Test1"
			});

			fieldQuery.GetFieldsForRDO(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTest.ChangeWebAPIPath(new RDOCustodianSynchronizer(fieldQuery, null));
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
				ArtifactGuids = new List<Guid> { Guid.Parse(RDOCustodianSynchronizer.CustodianFieldGuids.UniqueID) },
				Name = "Test1"
			});

			fieldQuery.GetFieldsForRDO(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTest.ChangeWebAPIPath(new RDOCustodianSynchronizer(fieldQuery, null));
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
