using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Moq;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class InstanceSettingsManagerTests : TestBase
    {
        private Mock<IRepositoryFactory> _repositoryFactoryFake;
        private Mock<IInstanceSettingRepository> _instanceSettingRepositoryFake;
        private InstanceSettingsManager _sut;

        private const string _FRIENDLY_NAME = "Friendly Name";
        private const string _RELATIVITY_AUTHENTICATION = "Relativity.Authentication";
        private const string _FRIENDLY_INSTANCE_NAME = "FriendlyInstanceName";

        private const string _RELATIVITY_CORE = "Relativity.Core";
        private const string _ALLOW_NO_SNAPSHOT_IMPORT = "AllowNoSnapshotImport";

        private const string _RESTRICT_REFERENTIAL_FILE_LINKS_ON_IMPORT = "RestrictReferentialFileLinksOnImport";

        [SetUp]
        public override void SetUp()
        {
            _repositoryFactoryFake = new Mock<IRepositoryFactory>();
            _instanceSettingRepositoryFake = new Mock<IInstanceSettingRepository>();
            
            _repositoryFactoryFake.Setup(m => m.GetInstanceSettingRepository()).Returns(_instanceSettingRepositoryFake.Object);

            _sut = new InstanceSettingsManager(_repositoryFactoryFake.Object);
        }

        [Test]
        public void TestRetriveCurrentInstanceFriendlyName()
        {
            //arrange
            _instanceSettingRepositoryFake.Setup(m => m.GetConfigurationValue(_RELATIVITY_AUTHENTICATION, _FRIENDLY_INSTANCE_NAME))
                .Returns(_FRIENDLY_NAME);

            //act
            string instanceFriendlyName = _sut.RetriveCurrentInstanceFriendlyName();

            //assert
            instanceFriendlyName.Should().Be(_FRIENDLY_NAME);
        }

        [Test]
        [TestCase("True", true)]
        [TestCase("False", false)]
        [TestCase(null, false)]
        public void TestRetrieveAllowNoSnapshotImport(string allowNoSnapshotImport, bool expectedResult)
        {
            //arrange
            _instanceSettingRepositoryFake.Setup(m => m.GetConfigurationValue(_RELATIVITY_CORE, _ALLOW_NO_SNAPSHOT_IMPORT))
                .Returns(allowNoSnapshotImport);

            //act
            bool result = _sut.RetrieveAllowNoSnapshotImport();

            //assert
            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("True", true)]
        [TestCase("False", false)]
        public void RetrieveRestrictReferentialFileLinksOnImport_ShouldReturnsValue_WhenSettingExists(string settingValue, bool expectedResult)
        {
            //arrange
            _instanceSettingRepositoryFake.Setup(m => m.GetConfigurationValue(_RELATIVITY_CORE, _RESTRICT_REFERENTIAL_FILE_LINKS_ON_IMPORT))
                .Returns(settingValue);

            //act
            bool result = _sut.RetrieveRestrictReferentialFileLinksOnImport();

            //assert
            result.Should().Be(expectedResult);
        }

        [Test]
        public void RetrieveRestrictReferentialFileLinksOnImport_ShouldReturnsFalse_WhenSettingIsInvalid()
        {
            //arrange
            _instanceSettingRepositoryFake.Setup(m => m.GetConfigurationValue(_RELATIVITY_CORE, _RESTRICT_REFERENTIAL_FILE_LINKS_ON_IMPORT))
                .Returns("Test");

            //act
            bool result = _sut.RetrieveRestrictReferentialFileLinksOnImport();

            //assert
            result.Should().BeFalse();
        }

        [Test]
        public void RetrieveRestrictReferentialFileLinksOnImport_ShouldReturnsFalse_WhenSettingDoesNotExist()
        {
            //act
            bool result = _sut.RetrieveRestrictReferentialFileLinksOnImport();

            //assert
            result.Should().BeFalse();
        }
    }
}