using System;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Common.Tests.Monitoring.Instrumentation
{
    [TestFixture, Category("Unit")]
    public class ExternalCallCompletedMessageTests
    {
        [Test]
        public void ItShouldSetProperServiceCallContext()
        {
            // arrange
            var sut = new ExternalCallCompletedMessage();
            var context = new InstrumentationServiceCallContext
            (
                serviceType: Guid.NewGuid().ToString(),
                serviceName: Guid.NewGuid().ToString(),
                operationName: Guid.NewGuid().ToString()
            );

            // act
            sut.SetCallContext(context);

            // assert
            Assert.AreEqual(context.ServiceType, sut.ServiceType);
            Assert.AreEqual(context.ServiceType, sut.CustomData[nameof(sut.ServiceType)]);

            Assert.AreEqual(context.ServiceName, sut.ServiceName);
            Assert.AreEqual(context.ServiceName, sut.CustomData[nameof(sut.ServiceName)]);

            Assert.AreEqual(context.OperationName, sut.OperationName);
            Assert.AreEqual(context.OperationName, sut.CustomData[nameof(sut.OperationName)]);
        }

        [Test]
        public void ItShouldSetProperJobCallContext()
        {
            // arrange
            const int workspaceId = 545432;
            var sut = new ExternalCallCompletedMessage();
            var context = new InstrumentationJobContext
            (
                jobId: 54543543,
                correlationId: Guid.NewGuid().ToString(),
                workspaceId: workspaceId
            );

            // act
            sut.SetJobContext(context);

            // assert
            Assert.AreEqual(context.JobId.ToString(), sut.JobID);
            Assert.AreEqual(context.JobId.ToString(), sut.CustomData[nameof(sut.JobID)]);

            Assert.AreEqual(context.CorrelationId, sut.CorrelationID);
            Assert.AreEqual(context.WorkspaceId, sut.WorkspaceID);
        }

        [Test]
        public void ItShouldSetProperSuccesData()
        {
            // arrange
            const long duration = 45242343;
            var sut = new ExternalCallCompletedMessage();

            // act
            sut.SetPropertiesForSuccess(duration);

            // assert
            Assert.AreEqual(duration, sut.Duration);
            Assert.AreEqual(duration, sut.CustomData[nameof(sut.Duration)]);

            Assert.IsFalse(sut.HasFailed);
            Assert.IsFalse((bool)sut.CustomData[nameof(sut.HasFailed)]);

            Assert.IsEmpty(sut.FailureReason);
            Assert.IsEmpty((string)sut.CustomData[nameof(sut.FailureReason)]);
        }

        [Test]
        public void ItShouldSetProperFailureData()
        {
            // arrange
            const long duration = 45624532;
            const string failureReason = "Timeout";
            var sut = new ExternalCallCompletedMessage();

            // act
            sut.SetPropertiesForFailure(duration, failureReason);

            // assert
            Assert.AreEqual(duration, sut.Duration);
            Assert.AreEqual(duration, sut.CustomData[nameof(sut.Duration)]);

            Assert.IsTrue(sut.HasFailed);
            Assert.IsTrue((bool)sut.CustomData[nameof(sut.HasFailed)]);

            Assert.AreEqual(failureReason, sut.FailureReason);
            Assert.AreEqual(failureReason, sut.CustomData[nameof(sut.FailureReason)]);
        }
    }
}
