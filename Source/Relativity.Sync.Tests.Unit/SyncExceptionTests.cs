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
			instance.CorrelationId.Should().BeNull();
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
			instance.CorrelationId.Should().BeNull();
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
			instance.CorrelationId.Should().BeNull();
			instance.InnerException.Should().Be(innerEx);
			instance.Message.Should().Be(expectedMessage);
		}

		[Test]
		public void ItShouldUseMessageAndCorrelationId()
		{
			const string expectedMessage = "message";
			const string correlationId = "id";

			// ACT
			SyncException instance = new SyncException(expectedMessage, correlationId);

			// ASSERT
			instance.InnerException.Should().BeNull();
			instance.CorrelationId.Should().Be(correlationId);
			instance.Message.Should().Be(expectedMessage);
		}

		[Test]
		public void ItShouldUseErrorMessageAndInnerExceptionAndCorrelationId()
		{
			const string expectedMessage = "message";
			const string correlationId = "id";
			Exception innerEx = new Exception("foo");

			// ACT
			SyncException instance = new SyncException(expectedMessage, innerEx, correlationId);

			// ASSERT
			instance.CorrelationId.Should().Be(correlationId);
			instance.InnerException.Should().Be(innerEx);
			instance.Message.Should().Be(expectedMessage);
		}

		[Test]
		public void ItShouldThrowExceptionWhenInfoIsNull()
		{
			SyncException sut = new SyncException("message", "correlation id");

			// ACT
			Action action = () => sut.GetObjectData(null, new StreamingContext());

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
		}
	}
}