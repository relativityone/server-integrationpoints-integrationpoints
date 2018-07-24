using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Custodian;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Artifact = kCura.Relativity.Client.Artifact;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
	[TestFixture]
	public class RdoEntitySynchronizerTests : TestBase
	{

		#region GetFields

		private string _settings;
		private IRSAPIClient _rsapiClient;
		private IHelper _helper;
		private RelativityFieldQuery _fieldQuery;
		private IImportJobFactory _importJobFactory;

		public static ImportApiFactory GetMockAPI(RelativityFieldQuery query)
		{
			var import = Substitute.For<Relativity.ImportAPI.IExtendedImportAPI>();
			var result = query.GetFieldsForRdo(0);
			var list = new List<Relativity.ImportAPI.Data.Field>();
			var mi = typeof(Relativity.ImportAPI.Data.Field).GetProperty("ArtifactID").GetSetMethod(true);
			foreach (var r in result)
			{
				var f = new Relativity.ImportAPI.Data.Field();
				mi.Invoke(f, new object[] { r.ArtifactID });
				list.Add(f);
			}

			import.GetWorkspaceFields(Arg.Any<int>(), Arg.Any<int>()).Returns(list);

			var mock = Substitute.For<ImportApiFactory>(Substitute.For<ITokenProvider>(), Substitute.For<IFederatedInstanceManager>(),
				Substitute.For<IHelper>(), Substitute.For<ISystemEventLoggingService>(), Substitute.For<ISerializer>());
			mock.GetImportAPI(Arg.Any<ImportSettings>()).Returns(import);
			return mock;
		}

		[OneTimeSetUp]
		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			_settings = JsonConvert.SerializeObject(new ImportSettings());
			_importJobFactory = new ImportJobFactory(Substitute.For<IMessageService>());
		}

		[SetUp]
		public override void SetUp()
		{
			_rsapiClient = Substitute.For<IRSAPIClient>();
			_helper = Substitute.For<IHelper>();
			_fieldQuery = Substitute.For<RelativityFieldQuery>(_rsapiClient, _helper);
		}

		[Test]
		public void GetFields_FieldsContainsFirstName_MakesFieldRequired()
		{
			//ARRANGE
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
				ArtifactGuids = new List<Guid> { Guid.Parse(EntityFieldGuids.FirstName) },
				Name = "Test1"
			});

			_fieldQuery.GetFieldsForRdo(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTests.ChangeWebAPIPath(new RdoEntitySynchronizer(_fieldQuery, GetMockAPI(_fieldQuery), _importJobFactory, _helper));
			var fields = sync.GetFields(new DataSourceProviderConfiguration(_settings));


			//ASSERT
			var field = fields.First(x => x.DisplayName.Equals("Test1"));
			Assert.AreEqual(true, field.IsRequired);
		}


		[Test]
		public void GetFields_FieldsContainsLastName_MakesFieldRequired()
		{
			//ARRANGE
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
				ArtifactGuids = new List<Guid> { Guid.Parse(EntityFieldGuids.LastName) },
				Name = "Test1"
			});

			_fieldQuery.GetFieldsForRdo(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTests.ChangeWebAPIPath(new RdoEntitySynchronizer(_fieldQuery, GetMockAPI(_fieldQuery), _importJobFactory, _helper));
			var fields = sync.GetFields(new DataSourceProviderConfiguration(_settings));


			//ASSERT
			var field = fields.First(x => x.DisplayName.Equals("Test1"));
			Assert.AreEqual(true, field.IsRequired);
		}

		[Test]
		public void GetFields_FieldsContainsUniqueID_OnlyUniqueIDSetForIdentifier()
		{
			//ARRANGE
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
				ArtifactGuids = new List<Guid> { Guid.Parse(EntityFieldGuids.UniqueID) },
				Name = "Test1"
			});

			_fieldQuery.GetFieldsForRdo(Arg.Any<int>()).Returns(artifacts);

			//ACT
			var sync = RdoSynchronizerTests.ChangeWebAPIPath(new RdoEntitySynchronizer(_fieldQuery, GetMockAPI(_fieldQuery), _importJobFactory, _helper));
			var fields = sync.GetFields(new DataSourceProviderConfiguration(_settings)).ToList();


			//ASSERT
			var idCount = fields.Count(x => x.IsIdentifier);
			Assert.AreEqual(1, idCount);
			var field = fields.Single(x => x.DisplayName.Equals("Test1"));
			Assert.AreEqual(true, field.IsIdentifier);
		}

		#endregion

	}
}