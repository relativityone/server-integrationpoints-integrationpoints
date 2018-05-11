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
		public void TestCreateTargetHelperIfFederatedInstanceIsNull()
		{
			//arrange
			IAPILog logger = Substitute.For<IAPILog>();
			var testInstance = new HelperFactory(_managerFactory, _contextContainerFactory, _tokenProvider, new IntegrationPointSerializer(logger));

			//act
			IHelper helper = testInstance.CreateTargetHelper(_sourceInstanceHelper, null, string.Empty);

			//assert
			Assert.That(helper, Is.EqualTo(_sourceInstanceHelper));
		}
	}
}
