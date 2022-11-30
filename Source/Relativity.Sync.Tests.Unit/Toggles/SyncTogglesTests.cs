using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Toggles.Service;
using Relativity.Toggles;

namespace Relativity.Sync.Tests.Unit.Toggles
{
    internal interface ITestToggle : IToggle
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
            _toggleProviderMock.Setup(x => x.IsEnabled<ITestToggle>()).Returns(true);

            // Act
            bool actual = _sut.IsEnabled<ITestToggle>();

            // Assert
            actual.Should().BeTrue();
            _toggleProviderMock.Verify(x => x.IsEnabled<ITestToggle>(), Times.Once);
        }

        [Test]
        public void IsEnabled_ShouldReturnValue_WhenValueIsCached()
        {
            // Arrange
            _toggleProviderMock.Setup(x => x.IsEnabled<ITestToggle>()).Returns(true);

            // Act
            bool result1 = _sut.IsEnabled<ITestToggle>();
            _toggleProviderMock.Setup(x => x.IsEnabled<ITestToggle>()).Returns(false);
            bool result2 = _sut.IsEnabled<ITestToggle>();

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeTrue();
            _toggleProviderMock.Verify(x => x.IsEnabled<ITestToggle>(), Times.Once);
        }
    }
}
