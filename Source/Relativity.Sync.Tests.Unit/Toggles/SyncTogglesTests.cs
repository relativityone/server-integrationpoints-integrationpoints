using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Toggles.Service;
using Relativity.Toggles;

namespace Relativity.Sync.Tests.Unit.Toggles
{
    internal interface TestToggle : IToggle
    {
    }

    [TestFixture]
    public class SyncTogglesTests
    {
        private Mock<IToggleProvider> _toggleProviderMock;

        private SyncToggles _sut;

        [SetUp]
        public void SetUp()
        {
            _toggleProviderMock = new Mock<IToggleProvider>();
            _sut = new SyncToggles(_toggleProviderMock.Object, new EmptyLogger());
        }

        [Test]
        public void IsEnabled_ShouldReturnValue_WhenValueIsNotCached()
        {
            // Arrange
            _toggleProviderMock.Setup(x => x.IsEnabled<TestToggle>()).Returns(true);

            // Act
            bool actual = _sut.IsEnabled<TestToggle>();

            // Assert
            actual.Should().BeTrue();
            _toggleProviderMock.Verify(x => x.IsEnabled<TestToggle>(), Times.Once);
        }

        [Test]
        public void IsEnabled_ShouldReturnValue_WhenValueIsCached()
        {
            // Arrange
            _toggleProviderMock.Setup(x => x.IsEnabled<TestToggle>()).Returns(true);

            // Act
            bool result1 = _sut.IsEnabled<TestToggle>();
            _toggleProviderMock.Setup(x => x.IsEnabled<TestToggle>()).Returns(false);
            bool result2 = _sut.IsEnabled<TestToggle>();

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            _toggleProviderMock.Verify(x => x.IsEnabled<TestToggle>(), Times.Once);
        }
    }
}
