using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.LibraryApplication;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Pipelines
{
    [TestFixture]
    public class IAPIv2RunCheckerTests
    {
        private IIAPIv2RunChecker _sut;
        private Mock<ISyncToggles> _togglesMock;
        private Mock<IIAPIv2RunCheckerConfiguration> _runCheckerConfig;
        private Mock<IFieldMappings> _fieldMappingsMock;
        private Mock<IObjectFieldTypeRepository> _objectFieldTypeRepositoryMock;
        private Mock<IDestinationServiceFactoryForAdmin> _serviceFactoryMock;
        private Mock<IApplicationInstallManager> _appInstallManagerMock;

        private const int _DEST_WORKSPACE_ID = 111;
        private static readonly Guid ImportAppGuid = new Guid("21f65fdc-3016-4f2b-9698-de151a6186a2");

        [SetUp]
        public void SetUp()
        {
            _togglesMock = new Mock<ISyncToggles>();
            _runCheckerConfig = new Mock<IIAPIv2RunCheckerConfiguration>();
            _runCheckerConfig.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_DEST_WORKSPACE_ID);
            _fieldMappingsMock = new Mock<IFieldMappings>();
            _objectFieldTypeRepositoryMock = new Mock<IObjectFieldTypeRepository>();
            _appInstallManagerMock = new Mock<IApplicationInstallManager>();
            _appInstallManagerMock
                .Setup(x => x.GetStatusAsync(_DEST_WORKSPACE_ID, ImportAppGuid, It.IsAny<bool>()))
                .ReturnsAsync(new GetInstallStatusResponse()
                {
                    InstallStatus = new InstallStatus()
                    {
                        Code = InstallStatusCode.Completed
                    }
                });
            _serviceFactoryMock = new Mock<IDestinationServiceFactoryForAdmin>();
            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IApplicationInstallManager>()).ReturnsAsync(_appInstallManagerMock.Object);

            SetUpInitialValuesForPositiveCheck();

            _sut = new IAPIv2RunChecker(_runCheckerConfig.Object, _togglesMock.Object, _fieldMappingsMock.Object, _objectFieldTypeRepositoryMock.Object, _serviceFactoryMock.Object, new EmptyLogger());
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnTrue_IfAllConditionsForGoldFlowAreMet()
        {
            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void ShouldBeUsed_ShouldCheckConditionsOnlyOnceAndThenStoreIt()
        {
            // Arrange
            _sut.ShouldBeUsed();

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeTrue();
            _togglesMock.Verify(x => x.IsEnabled<EnableIAPIv2Toggle>(), Times.Once);
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfIAPIToggleIsDisabled()
        {
            // Arrange
            _togglesMock.Setup(x => x.IsEnabled<EnableIAPIv2Toggle>()).Returns(false);

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfTransferredObjectIsNotDocument()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Client);

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfJobIsRetried()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.IsRetried).Returns(true);

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfJobIsDrainStopped()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.IsDrainStopped).Returns(true);

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfJobHasLongTextFieldsMapped()
        {
            // Arrange
            Dictionary<string, RelativityDataType> dataTypes = new Dictionary<string, RelativityDataType>();
            dataTypes.Add("testValue", RelativityDataType.LongText);
            _objectFieldTypeRepositoryMock.Setup(x => x.GetRelativityDataTypesForFieldsByFieldNameAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ICollection<string>>(),
                CancellationToken.None)).ReturnsAsync(dataTypes);

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfJobHasModeSetToCopyFiles()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.NativeBehavior).Returns(ImportNativeFileCopyMode.CopyFiles);

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfImagesAreImported()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.ImageImport).Returns(true);

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_IfDocumentTaggingIsEnabled()
        {
            // Arrange
            _runCheckerConfig.SetupGet(x => x.EnableTagging).Returns(true);

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(InstallStatusCode.Canceled)]
        [TestCase(InstallStatusCode.Failed)]
        [TestCase(InstallStatusCode.InProgress)]
        [TestCase(InstallStatusCode.Pending)]
        [TestCase(InstallStatusCode.Unknown)]
        public void ShouldBeUsed_ShouldReturnFalse_WhenImportAppIsNotFullyInstalled(InstallStatusCode installStatusCode)
        {
            // Arrange
            _appInstallManagerMock
                .Setup(x => x.GetStatusAsync(_DEST_WORKSPACE_ID, ImportAppGuid, It.IsAny<bool>()))
                .ReturnsAsync(new GetInstallStatusResponse()
                {
                    InstallStatus = new InstallStatus()
                    {
                        Code = installStatusCode
                    }
                });

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_WhenImportAppIsNotInstalled()
        {
            // Arrange
            _appInstallManagerMock
                .Setup(x => x.GetStatusAsync(_DEST_WORKSPACE_ID, ImportAppGuid, It.IsAny<bool>()))
                .Throws<NotFoundException>();

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldBeUsed_ShouldReturnFalse_WhenCannotCheckIfImportIsInstalled()
        {
            // Arrange
            _appInstallManagerMock
                .Setup(x => x.GetStatusAsync(_DEST_WORKSPACE_ID, ImportAppGuid, It.IsAny<bool>()))
                .Throws<Exception>();

            // Act
            bool result = _sut.ShouldBeUsed();

            // Assert
            result.Should().BeFalse();
        }

        private void SetUpInitialValuesForPositiveCheck()
        {
            _togglesMock.Setup(x => x.IsEnabled<EnableIAPIv2Toggle>()).Returns(true);
            _runCheckerConfig.SetupGet(x => x.ImageImport).Returns(false);
            _runCheckerConfig.SetupGet(x => x.IsDrainStopped).Returns(false);
            _runCheckerConfig.SetupGet(x => x.IsRetried).Returns(false);
            _runCheckerConfig.SetupGet(x => x.NativeBehavior).Returns(ImportNativeFileCopyMode.DoNotImportNativeFiles);
            _runCheckerConfig.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Document);

            FieldMap fieldMap = new FieldMap
            {
                SourceField = new FieldEntry() { DisplayName = "TestName" },
                DestinationField = new FieldEntry(),
                FieldMapType = FieldMapType.None
            };

            IList<FieldMap> fieldMaps = new List<FieldMap> { fieldMap };
            _fieldMappingsMock.Setup(x => x.GetFieldMappings()).Returns(fieldMaps);

            Dictionary<string, RelativityDataType> dataTypes = new Dictionary<string, RelativityDataType>();
            dataTypes.Add("testValue", RelativityDataType.Date);
            _objectFieldTypeRepositoryMock.Setup(x => x.GetRelativityDataTypesForFieldsByFieldNameAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ICollection<string>>(),
                CancellationToken.None)).ReturnsAsync(dataTypes);
        }
    }
}
