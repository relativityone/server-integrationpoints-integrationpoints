using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Factories
{
	[TestFixture]
	public class HelperFactoryTests : TestBase
	{
		private IManagerFactory _managerFactory;
		private IContextContainerFactory _contextContainerFactory;
		private ITokenProvider _tokenProvider;
		private IFederatedInstanceManager _federatedInstanceManager;
		private IHelper _sourceInstanceHelper;

		public override void SetUp()
		{
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_tokenProvider = Substitute.For<ITokenProvider>();
			_federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			_sourceInstanceHelper = Substitute.For<IHelper>();
		}

		[Test]
		public void TestCreateOAuthClientHelper()
		{
			//arrange
			IContextContainer sourceContextContainer = Substitute.For<IContextContainer>();
			_contextContainerFactory.CreateContextContainer(_sourceInstanceHelper).Returns(sourceContextContainer);
			_managerFactory.CreateFederatedInstanceManager(sourceContextContainer).Returns(_federatedInstanceManager);

			string instanceUrl = "http://hostname/";
			string rsapiUrl = "http://hostname/Relativity.Services";
			string keplerUrl = "http://hostname/Relativity.REST/api/";

			_federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(Arg.Any<int>()).Returns(new FederatedInstanceDto()
			{
				InstanceUrl = instanceUrl,
				RsapiUrl = rsapiUrl,
				KeplerUrl = keplerUrl
			});

			var testInstance = new HelperFactory(_managerFactory, _contextContainerFactory, _tokenProvider, new IntegrationPointSerializer());

			//act
			IHelper helper = testInstance.CreateTargetHelper(_sourceInstanceHelper, 1000, "{}");

			//assert
			Assert.That(helper.GetServicesManager(), Is.Not.Null);
			Assert.That(helper.GetServicesManager().GetServicesURL().AbsoluteUri, Is.EqualTo(rsapiUrl));
			Assert.That(helper.GetServicesManager().GetRESTServiceUrl().AbsoluteUri, Is.EqualTo(keplerUrl));
		}

		[Test]
		public void TestCreateTargetHelperIfFederatedInstanceIsNull()
		{
			//arrange
			var testInstance = new HelperFactory(_managerFactory, _contextContainerFactory, _tokenProvider, new IntegrationPointSerializer());

			//act
			IHelper helper = testInstance.CreateTargetHelper(_sourceInstanceHelper, null, string.Empty);

			//assert
			Assert.That(helper, Is.EqualTo(_sourceInstanceHelper));
		}
	}
}
