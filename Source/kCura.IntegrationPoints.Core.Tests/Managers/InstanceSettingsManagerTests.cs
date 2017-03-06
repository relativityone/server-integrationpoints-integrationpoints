using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class InstanceSettingsManagerTests : TestBase
	{
		private const string FRIENDLY_NAME = "Friendly Name";
		private const string RELATIVITY_AUTHENTICATION = "Relativity.Authentication";
		private const string FRIENDLY_INSTANCE_NAME = "FriendlyInstanceName";
		private IRepositoryFactory _repositoryFactory;
		private IInstanceSettingRepository _instanceSettingRepository;

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();
			_instanceSettingRepository.GetConfigurationValue(RELATIVITY_AUTHENTICATION,
				FRIENDLY_INSTANCE_NAME).Returns(FRIENDLY_NAME);
			
			_repositoryFactory.GetInstanceSettingRepository().Returns(_instanceSettingRepository);
		}

		[Test]
		public void TestRetriveCurrentInstanceFriendlyName()
		{
			//arrange
			var testInstance = new InstanceSettingsManager(_repositoryFactory);
			//act
			var instanceFriendlyName = testInstance.RetriveCurrentInstanceFriendlyName();
			//assert
			Assert.AreEqual(FRIENDLY_NAME, instanceFriendlyName);
		}
	}
}