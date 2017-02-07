using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class FederatedInstanceManagerTests : TestBase
	{
		private int artifactTypeId = 2048;
		private IRepositoryFactory _repositoryFactory;
		private IArtifactTypeRepository _artifactTypeRepository;
		private IFederatedInstanceRepository _federatedInstanceRepository;
		private IServiceUrlRepository _serviceUrlRepository;

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_artifactTypeRepository = Substitute.For<IArtifactTypeRepository>();
			_federatedInstanceRepository = Substitute.For<IFederatedInstanceRepository>();

			_artifactTypeRepository.GetArtifactTypeIdFromArtifactTypeName("FederatedInstance").Returns(artifactTypeId);
			_repositoryFactory.GetArtifactTypeRepository().Returns(_artifactTypeRepository);
			_repositoryFactory.GetFederatedInstanceRepository(artifactTypeId).Returns(_federatedInstanceRepository);

			_serviceUrlRepository = Substitute.For<IServiceUrlRepository>();
			_repositoryFactory.GetServiceUrlRepository().Returns(_serviceUrlRepository);
		}

		[Test]
		public void TestLocalInstance()
		{
			var testInstance = new FederatedInstanceManager(_repositoryFactory);

			var federatedInstance = testInstance.RetrieveFederatedInstance(null);

			Assert.That(federatedInstance, Is.EqualTo(FederatedInstanceManager.LocalInstance));
		}

		[Test]
		public void TestRetrieveFederatedInstance()
		{
			//arrange
			var federatedInstanceArtifactId = 1024;
			var testInstance = new FederatedInstanceManager(_repositoryFactory);

			string name = "FederatedInstance1";
			string instanceUrl = "http://hostname/";
			string rsapiUrl = "http://hostname/Relativity.Services";
			string keplerUrl = "http://hostname/Relativity.REST/api/";
			string webApiUrl = "http://hostname/RelativityWebAPI/";

			_federatedInstanceRepository.RetrieveFederatedInstance(federatedInstanceArtifactId)
				.Returns(new FederatedInstanceDto()
				{
					Name = name,
					ArtifactId = federatedInstanceArtifactId,
					InstanceUrl = instanceUrl
				});

			_serviceUrlRepository.RetrieveInstanceUrlCollection(new Uri(instanceUrl)).Returns(new InstanceUrlCollectionDTO()
			{
				RsapiUrl = rsapiUrl,
				KeplerUrl = keplerUrl,
				WebApiUrl = webApiUrl
			});

			//act
			var federatedInstance = testInstance.RetrieveFederatedInstance(federatedInstanceArtifactId);

			//assert
			Assert.That(federatedInstance, Is.Not.Null);
			Assert.That(federatedInstance.Name, Is.EqualTo(name));
			Assert.That(federatedInstance.ArtifactId, Is.EqualTo(federatedInstanceArtifactId));
			Assert.That(federatedInstance.InstanceUrl, Is.EqualTo(instanceUrl));
			Assert.That(federatedInstance.RsapiUrl, Is.EqualTo(rsapiUrl));
			Assert.That(federatedInstance.KeplerUrl, Is.EqualTo(keplerUrl));
			Assert.That(federatedInstance.WebApiUrl, Is.EqualTo(webApiUrl));
		}

		[Test]
		public void TestRetrieveAllHasLocalInstance()
		{
			//arrange
			var testInstance = new FederatedInstanceManager(_repositoryFactory);

			int artifactId1 = 4096;
			string name1 = "FederatedInstance1";
			string instanceUrl1 = "http://hostname1/";

			int artifactId2 = 8192;
			string name2 = "FederatedInstance2";
			string instanceUrl2 = "http://hostname2/";

			_federatedInstanceRepository.RetrieveAll().Returns(new List<FederatedInstanceDto>
			{
				new FederatedInstanceDto() { ArtifactId = artifactId1, Name = name1, InstanceUrl = instanceUrl1},
				new FederatedInstanceDto() { ArtifactId = artifactId2, Name = name2, InstanceUrl = instanceUrl2}
			});

			_serviceUrlRepository.RetrieveInstanceUrlCollection(new Uri(instanceUrl1)).Returns(new InstanceUrlCollectionDTO()
			{
				RsapiUrl = "http://hostname1/Relativity.Services",
				KeplerUrl = "http://hostname1/Relativity.REST/api/",
				WebApiUrl = "http://hostname1/RelativityWebAPI/"
			});

			_serviceUrlRepository.RetrieveInstanceUrlCollection(new Uri(instanceUrl2)).Returns(new InstanceUrlCollectionDTO()
			{
				RsapiUrl = "http://hostname2/Relativity.Services",
				KeplerUrl = "http://hostname2/Relativity.REST/api/",
				WebApiUrl = "http://hostname2/RelativityWebAPI/"
			});

			//act
			var federatedInstances = testInstance.RetrieveAll().ToList();

			Assert.That(federatedInstances, Is.Not.Null);
			Assert.That(federatedInstances.Count(), Is.EqualTo(3));
			Assert.That(federatedInstances[0].Name, Is.EqualTo("This Instance"));
			Assert.That(federatedInstances[0].ArtifactId, Is.EqualTo(null));
			Assert.That(federatedInstances[1].Name, Is.EqualTo(name1));
			Assert.That(federatedInstances[1].ArtifactId, Is.EqualTo(artifactId1));
			Assert.That(federatedInstances[2].Name, Is.EqualTo(name2));
			Assert.That(federatedInstances[2].ArtifactId, Is.EqualTo(artifactId2));
		}
	}
}