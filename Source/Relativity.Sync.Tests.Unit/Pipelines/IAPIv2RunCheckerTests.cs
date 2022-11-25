using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
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

        [SetUp]
        public void SetUp()
        {
            _togglesMock = new Mock<ISyncToggles>();
            _runCheckerConfig = new Mock<IIAPIv2RunCheckerConfiguration>();
            _fieldMappingsMock = new Mock<IFieldMappings>();
            _objectFieldTypeRepositoryMock = new Mock<IObjectFieldTypeRepository>();

            SetUpInitialValuesForPositiveCheck();

            _sut = new IAPIv2RunChecker(_runCheckerConfig.Object, _togglesMock.Object, _fieldMappingsMock.Object, _objectFieldTypeRepositoryMock.Object, new EmptyLogger());
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
