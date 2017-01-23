using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
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
		private IOAuthClientManager _oAuthClientManager;

		public override void SetUp()
		{
			_managerFactory = Substitute.For<IManagerFactory>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_tokenProvider = Substitute.For<ITokenProvider>();
			_federatedInstanceManager = Substitute.For<IFederatedInstanceManager>();
			_sourceInstanceHelper = Substitute.For<IHelper>();
			_oAuthClientManager = Substitute.For<IOAuthClientManager>();
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

			_federatedInstanceManager.RetrieveFederatedInstance(Arg.Any<int>()).Returns(new FederatedInstanceDto()
			{
				InstanceUrl = instanceUrl,
				RsapiUrl = rsapiUrl,
				KeplerUrl = keplerUrl
			});
			_managerFactory.CreateOAuthClientManager(sourceContextContainer).Returns(_oAuthClientManager);
			_oAuthClientManager.RetrieveOAuthClientForFederatedInstance(Arg.Any<int>()).Returns(new OAuthClientDto()
			{
				ClientId = "client123",
				ClientSecret = "asdfghjkl"
			});
			
			var testInstance = new HelperFactory(_managerFactory, _contextContainerFactory, _tokenProvider);

			//act
			IHelper helper = testInstance.CreateOAuthClientHelper(_sourceInstanceHelper, 1000);

			//assert
			Assert.That(helper.GetServicesManager(), Is.Not.Null);
			Assert.That(helper.GetServicesManager().GetServicesURL().AbsoluteUri, Is.EqualTo(rsapiUrl));
			Assert.That(helper.GetServicesManager().GetRESTServiceUrl().AbsoluteUri, Is.EqualTo(keplerUrl));
		}
	}
}
