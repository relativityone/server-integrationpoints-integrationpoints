using System;
using FluentAssertions;
using kCura.IntegrationPoints.EventHandlers.Context;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Context
{
    [TestFixture, Category("Unit")]
    public class EventHandlerWorkspaceContextServiceTests
    {
        private Mock<IEHHelper> _helperMock;
        private EventHandlerWorkspaceContextService _sut;

        [SetUp]
        public void Setup()
        {
            _helperMock = new Mock<IEHHelper>();
            _sut = new EventHandlerWorkspaceContextService(_helperMock.Object);
        }

        [Test]
        public void GetWorkspaceID_ShouldReturnWorkspaceIDAndCallHelperOnce()
        {
            // arrange
            const int expectedWorkspaceID = 1001;
            _helperMock
                .Setup(x => x.GetActiveCaseID())
                .Returns(expectedWorkspaceID);

            // act
            int actualWorkspaceID = _sut.GetWorkspaceID();

            // assert
            actualWorkspaceID.Should().Be(actualWorkspaceID);
            _helperMock.Verify(x => x.GetActiveCaseID(), Times.Once);
        }

        [Test]
        public void GetWorkspaceID_ShouldThrowWhenHelperThrows()
        {
            // arrange
            var sut = new EventHandlerWorkspaceContextService(helper: null);

            // act
            Action action = () => sut.GetWorkspaceID();

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void GetWorkspaceID_ShouldThrowWhenHelperNotInitialized()
        {
            // arrange
            var exception = new Exception("test 123");
            _helperMock
                .Setup(x => x.GetActiveCaseID())
                .Throws(exception);

            // act
            Action action = () => _sut.GetWorkspaceID();

            // assert
            action.ShouldThrow<Exception>().WithMessage(exception.Message);
        }
    }
}
