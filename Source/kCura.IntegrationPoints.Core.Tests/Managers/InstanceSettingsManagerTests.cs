using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class InstanceSettingsManagerTests : TestBase
	{
		private const string _FRIENDLY_NAME = "Friendly Name";
		private const string _RELATIVITY_AUTHENTICATION = "Relativity.Authentication";
		private const string _FRIENDLY_INSTANCE_NAME = "FriendlyInstanceName";

		private const string _RELATIVITY_CORE = "Relativity.Core";
		private const string _ALLOW_NO_SNAPSHOT_IMPORT = "AllowNoSnapshotImport";

		private IRepositoryFactory _repositoryFactory;
		private IInstanceSettingRepository _instanceSettingRepository;
		private InstanceSettingsManager _instance;

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_instanceSettingRepository = Substitute.For<IInstanceSettingRepository>();
			
			_repositoryFactory.GetInstanceSettingRepository().Returns(_instanceSettingRepository);

			_instance = new InstanceSettingsManager(_repositoryFactory);
		}

		[Test]
		public void TestRetriveCurrentInstanceFriendlyName()
		{
			//arrange
			_instanceSettingRepository.GetConfigurationValue(_RELATIVITY_AUTHENTICATION,
				_FRIENDLY_INSTANCE_NAME).Returns(_FRIENDLY_NAME);

			//act
			var instanceFriendlyName = _instance.RetriveCurrentInstanceFriendlyName();
			//assert
			Assert.AreEqual(_FRIENDLY_NAME, instanceFriendlyName);
		}

		[Test]
		[TestCase("True", true)]
		[TestCase("False", false)]
		[TestCase(null, false)]
		public void TestRetrieveAllowNoSnapshotImport(string allowNoSnapshotImport, bool expectedResult)
		{
			//arrange
			_instanceSettingRepository.GetConfigurationValue(_RELATIVITY_CORE,
				_ALLOW_NO_SNAPSHOT_IMPORT).Returns(allowNoSnapshotImport);

			//act
			var result = _instance.RetrieveAllowNoSnapshotImport();

			//assert
			Assert.That(result, Is.EqualTo(expectedResult));
		}
	}
}