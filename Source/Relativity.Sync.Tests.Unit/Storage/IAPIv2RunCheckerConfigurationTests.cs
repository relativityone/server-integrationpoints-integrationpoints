using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;
using IConfiguration = Relativity.Sync.Storage.IConfiguration;

namespace Relativity.Sync.Tests.Unit.Storage
{
    internal class IAPIv2RunCheckerConfigurationTests : ConfigurationTestBase
    {
        private IAPIv2RunCheckerConfiguration _sut;
        private Mock<IFieldManager> _fieldManagerMock;
        private Mock<IConfiguration> _configurationMock;

        [SetUp]
        public void SetUp()
        {
            _fieldManagerMock = new Mock<IFieldManager>();
            _configurationMock = new Mock<IConfiguration>();
        }

        [Test]
        public void HasLongTextFields_ShouldBeTrue_IfAnyOfMappedFieldsAreLongTextType()
        {
            // Arrange
            IList<FieldInfoDto> mappedTypes = new List<FieldInfoDto>();
            mappedTypes.Add(new FieldInfoDto(SpecialFieldType.None, "test field", string.Empty, false, true) { RelativityDataType = RelativityDataType.Date });
            mappedTypes.Add(new FieldInfoDto(SpecialFieldType.None, "test field", string.Empty, false, true) { RelativityDataType = RelativityDataType.LongText });

            _fieldManagerMock.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mappedTypes);

            // Act
            _sut = new IAPIv2RunCheckerConfiguration(_configurationMock.Object, _fieldManagerMock.Object);

            // Assert
            _sut.HasLongTextFields.Should().Be(true);
        }

        [Test]
        public void HasLongTextFields_ShouldBeFalse_IfThereIsNoLongTextTypeFieldsMapped()
        {
            // Arrange
            IList<FieldInfoDto> mappedTypes = new List<FieldInfoDto>();
            mappedTypes.Add(new FieldInfoDto(SpecialFieldType.None, "test field", string.Empty, false, true) { RelativityDataType = RelativityDataType.Date });

            _fieldManagerMock.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mappedTypes);

            // Act
            _sut = new IAPIv2RunCheckerConfiguration(_configurationMock.Object, _fieldManagerMock.Object);

            // Assert
            _sut.HasLongTextFields.Should().Be(false);
        }
    }
}
