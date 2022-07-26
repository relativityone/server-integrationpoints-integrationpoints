using System;
using System.Runtime.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class SyncExceptionTests
    {
        [Test]
        public void DefaultConstructor()
        {
            string expectedMessage = $"Exception of type '{typeof(SyncException).FullName}' was thrown.";

            // ACT
            SyncException instance = new SyncException();

            // ASSERT
            instance.WorkflowId.Should().BeNull();
            instance.InnerException.Should().BeNull();
            instance.Message.Should().Be(expectedMessage);
        }

        [Test]
        public void ItShouldUseErrorMessage()
        {
            const string expectedMessage = "message";

            // ACT
            SyncException instance = new SyncException(expectedMessage);

            // ASSERT
            instance.WorkflowId.Should().BeNull();
            instance.InnerException.Should().BeNull();
            instance.Message.Should().Be(expectedMessage);
        }

        [Test]
        public void ItShouldUseErrorMessageAndInnerException()
        {
            const string expectedMessage = "message";
            Exception innerEx = new Exception("foo");

            // ACT
            SyncException instance = new SyncException(expectedMessage, innerEx);

            // ASSERT
            instance.WorkflowId.Should().BeNull();
            instance.InnerException.Should().Be(innerEx);
            instance.Message.Should().Be(expectedMessage);
        }

        [Test]
        public void ItShouldUseMessageAndWorkflowId()
        {
            const string expectedMessage = "message";
            const string workflowId = "id";

            // ACT
            SyncException instance = new SyncException(expectedMessage, workflowId);

            // ASSERT
            instance.InnerException.Should().BeNull();
            instance.WorkflowId.Should().Be(workflowId);
            instance.Message.Should().Be(expectedMessage);
        }

        [Test]
        public void ItShouldUseErrorMessageAndInnerExceptionAndWorkflowId()
        {
            const string expectedMessage = "message";
            const string workflowId = "id";
            Exception innerEx = new Exception("foo");

            // ACT
            SyncException instance = new SyncException(expectedMessage, innerEx, workflowId);

            // ASSERT
            instance.WorkflowId.Should().Be(workflowId);
            instance.InnerException.Should().Be(innerEx);
            instance.Message.Should().Be(expectedMessage);
        }

        [Test]
        public void ItShouldThrowExceptionWhenInfoIsNull()
        {
            SyncException sut = new SyncException("message", "workflow id");

            // ACT
            Action action = () => sut.GetObjectData(null, new StreamingContext());

            // ASSERT
            action.Should().Throw<ArgumentNullException>();
        }
    }
}