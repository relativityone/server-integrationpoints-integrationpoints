using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.Apps.Common.Config;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Artifact = kCura.Relativity.Client.Artifact;
using Assert = NUnit.Framework.Assert;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Unit
{
	[TestFixture]
	public class RdoSynchronizerTest
	{
		public RdoSynchronizer ChangeWebAPIPath(RdoSynchronizer synchronizer)
		{
			var prop = synchronizer.GetType().GetProperty("WebAPIPath");
			prop.SetValue(synchronizer, "Mock value");
			return synchronizer;
		}

		[Test]
		public void GetRightCountOfFieldsWithSystemAndArtifactFeildsRemoved()
		{
			//ARRANGE
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var fieldMock = NSubstitute.Substitute.For<RelativityFieldQuery>(client);
			var rdoQuery = NSubstitute.Substitute.For<RelativityRdoQuery>(client);
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectType
			{
				ArtifactTypeID = 1,
				DescriptorArtifactTypeID = 1,
				Name = "Document"
			});
			//
			var rdoSyncronizer = ChangeWebAPIPath(new RdoSynchronizer(fieldMock, rdoQuery));
			var options = new ImportSettings();
			options.ArtifactTypeId = 1268820;
			fieldMock.GetFieldsForRDO(Arg.Any<int>()).Returns(new List<Artifact>
			{
				 new Artifact {Name = "Name", ArtifactID = 1},
				new Artifact {Name = "System Created On", ArtifactID = 2},
				new Artifact {Name = "Date Modified On", ArtifactID = 3},
				new Artifact {Name = "User", ArtifactID = 4},
				new Artifact {Name = "Artifact ID", ArtifactID = 5}
			});

			//ACT
			var str = JsonConvert.SerializeObject(options);
			var numberOfFields = rdoSyncronizer.GetFields(str).Count();
			//ASSERT

			Assert.AreEqual(3, numberOfFields);
		}


		[Test]
		public void GetRightDataInFieldsWithSystemAndArtifactFeildsRemoved()
		{
			//ARRANGE
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var fieldMock = NSubstitute.Substitute.For<RelativityFieldQuery>(client);
			var rdoQuery = NSubstitute.Substitute.For<RelativityRdoQuery>(client);
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectType
			{
				ArtifactTypeID = 1,
				DescriptorArtifactTypeID = 1,
				Name = "Document"
			});
			var rdoSyncronizer = ChangeWebAPIPath(new RdoSynchronizer(fieldMock, rdoQuery));
			var options = new ImportSettings { ArtifactTypeId = 1268820 };
			fieldMock.GetFieldsForRDO(Arg.Any<int>()).Returns(new List<Artifact>
			{
				new Artifact {Name = "Name", ArtifactID = 1},
				new Artifact {Name = "System Created On", ArtifactID = 2},
				new Artifact {Name = "Date Modified On", ArtifactID = 3},
				new Artifact {Name = "User", ArtifactID = 4},
				new Artifact {Name = "Artifact ID", ArtifactID = 5}
			});
			var expectedFieldEntry = new List<FieldEntry>
			{
				new FieldEntry {DisplayName = "Name", FieldIdentifier = "1"},
				new FieldEntry {DisplayName = "Date Modified On", FieldIdentifier = "3"},
				new FieldEntry {DisplayName = "User", FieldIdentifier = "4"},
			};

			//ACT
			var str = JsonConvert.SerializeObject(options);
			var listOfFieldEntry = rdoSyncronizer.GetFields(str).ToList();


			//ASSERT
			Assert.AreEqual(expectedFieldEntry.Count, listOfFieldEntry.Count);
			for (var i = 0; i < listOfFieldEntry.Count; i++)
			{
				Assert.AreEqual(listOfFieldEntry[i].DisplayName, expectedFieldEntry[i].DisplayName);
				Assert.AreEqual(listOfFieldEntry[i].FieldIdentifier, expectedFieldEntry[i].FieldIdentifier);
			}
		}

		[Test]
		public void GetRightCountOfFields()
		{
			//ARRANGE
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var fieldMock = NSubstitute.Substitute.For<RelativityFieldQuery>(client);
			//
			var rdoQuery = NSubstitute.Substitute.For<RelativityRdoQuery>(client);
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectType
				 {
					 ArtifactTypeID = 1,
					 DescriptorArtifactTypeID = 1,
					 Name = "Document"
				 });
			var rdoSyncronizer = ChangeWebAPIPath(new RdoSynchronizer(fieldMock, rdoQuery));
			var options = new ImportSettings();
			options.ArtifactTypeId = 1268820;
			fieldMock.GetFieldsForRDO(Arg.Any<int>()).Returns(new List<Artifact>
			{
				 new Artifact {Name = "Name", ArtifactID = 1},
				new Artifact {Name = "Value", ArtifactID = 2},
				new Artifact {Name = "Date Modified On", ArtifactID = 3},
				new Artifact {Name = "User", ArtifactID = 4},
				new Artifact {Name = "FirstName", ArtifactID = 5}
			});


			//ACT
			var str = JsonConvert.SerializeObject(options);
			var numberOfFields = rdoSyncronizer.GetFields(str).Count();
			//ASSERT

			Assert.AreEqual(5, numberOfFields);
		}


		[Test]
		public void GetRightDataInFields()
		{
			//ARRANGEk
			var client = NSubstitute.Substitute.For<IRSAPIClient>();
			var fieldMock = NSubstitute.Substitute.For<RelativityFieldQuery>(client);
			var rdoQuery = NSubstitute.Substitute.For<RelativityRdoQuery>(client);
			rdoQuery.GetObjectType(Arg.Any<int>()).Returns(new ObjectType
			{
				ArtifactTypeID = 1,
				DescriptorArtifactTypeID = 1,
				Name = "Document"
			});
			var rdoSyncronizer = ChangeWebAPIPath(new RdoSynchronizer(fieldMock, rdoQuery));
			var options = new ImportSettings { ArtifactTypeId = 1268820 };
			fieldMock.GetFieldsForRDO(Arg.Any<int>()).Returns(new List<Artifact>
			{
				new Artifact {Name = "Name", ArtifactID = 1},
				new Artifact {Name = "Value", ArtifactID = 2},
				new Artifact {Name = "Date Modified On", ArtifactID = 3},
				new Artifact {Name = "User", ArtifactID = 4},
				new Artifact {Name = "FirstName", ArtifactID = 5}
			});
			var expectedFieldEntry = new List<FieldEntry>
			{
				new FieldEntry {DisplayName = "Name", FieldIdentifier = "1"},
				new FieldEntry {DisplayName = "Value", FieldIdentifier = "2"},
				new FieldEntry {DisplayName = "Date Modified On", FieldIdentifier = "3"},
				new FieldEntry {DisplayName = "User", FieldIdentifier = "4"},
				new FieldEntry {DisplayName = "FirstName", FieldIdentifier = "5"}
			};

			//ACT
			var str = JsonConvert.SerializeObject(options);
			var listOfFieldEntry = rdoSyncronizer.GetFields(str).ToList();


			//ASSERT
			Assert.AreEqual(expectedFieldEntry.Count, listOfFieldEntry.Count);
			for (var i = 0; i < listOfFieldEntry.Count; i++)
			{
				Assert.AreEqual(listOfFieldEntry[i].DisplayName, expectedFieldEntry[i].DisplayName);
				Assert.AreEqual(listOfFieldEntry[i].FieldIdentifier, expectedFieldEntry[i].FieldIdentifier);
			}
		}


	}
}
