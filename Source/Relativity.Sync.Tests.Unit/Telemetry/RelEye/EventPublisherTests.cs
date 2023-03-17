using System.Collections.Generic;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.RelEye;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Telemetry.RelEye
{
    internal class EventPublisherTests
    {
        private Mock<IAPMClient> _apmMock;
        private SyncJobParameters _params;

        private IEventPublisher _sut;

        private IFixture _fxt = FixtureFactory.Create();

        [SetUp]
        public void SetUp()
        {
            _params = _fxt.Create<SyncJobParameters>();

            _apmMock = new Mock<IAPMClient>();

            Mock<IAPILog> log = new Mock<IAPILog>();

            _sut = new EventPublisher(_apmMock.Object, _params, log.Object);
        }

        [Test]
        public void Publish_ShouldSendCommonAttributesOnEveryCall()
        {
            // Arrange
            var testEvent = _fxt.Create<TestEvent>();

            // Act
            _sut.Publish(testEvent);

            // Assert
            _apmMock.Verify(x => x.Count(
                It.Is<string>(y => y == testEvent.EventName),
                It.Is<Dictionary<string, object>>(y =>
                    y[Const.Names.R1TeamID] == Const.Values.R1TeamID &&
                    y[Const.Names.ServiceName] == Const.Values.ServiceName &&
                    y[Const.Names.WorkflowId] == _params.WorkflowId)));
        }

        private class TestEvent : EventBase<TestEvent>
        {
            public override string EventName => "test_event";
        }
    }
}
