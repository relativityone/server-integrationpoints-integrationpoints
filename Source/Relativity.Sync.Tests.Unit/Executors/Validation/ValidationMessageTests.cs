using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class ValidationMessageTests
	{
		[Test]
		public void ItShouldCreateValidationMessageWithErrorCode()
		{
			string shortMessage = "message";
			string errorCode = "error code";

			var validationMessage = new ValidationMessage(errorCode, shortMessage);

			validationMessage.ShortMessage.Should().Be(shortMessage);
			validationMessage.ErrorCode.Should().Be(errorCode);
		}
		[Test]
		public void ItShouldCreateValidationMessage()
		{
			string shortMessage = "message";

			var validationMessage = new ValidationMessage(shortMessage);

			validationMessage.ShortMessage.Should().Be(shortMessage);
			validationMessage.ErrorCode.Should().Be(string.Empty);
		}

		[Test]
		public void ItShouldNotThrowWhenComparingToNull()
		{
			string shortMessage = "message";
			string errorCode = "error code";

			var validationMessage = new ValidationMessage(errorCode, shortMessage);

			Func<bool> action = () => validationMessage.Equals(null);

			action.Should().NotThrow();
		}

		[Test]
		public void ItShouldNotThrowOnNulls()
		{
			var otherMessage = new ValidationMessage(null, null);
			
			var validationMessage = new ValidationMessage(null, null);

			Func<bool> action = () => validationMessage.Equals(otherMessage);

			action.Should().NotThrow();
		}

		[Test]
		public void ItShouldBeEqual()
		{
			string shortMessage = "message";
			string errorCode = "error code";
			var otherMessage = new ValidationMessage(errorCode, shortMessage);

			var validationMessage = new ValidationMessage(errorCode, shortMessage);

			bool result = validationMessage.Equals(otherMessage);

			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldNotBeEqualWhenMessagesDiffer()
		{
			string theObjectMessage = "message";
			string theObjectErrorCode = "error code";
			string otherObjectMessage = "other message";
			var otherMessage = new ValidationMessage(theObjectErrorCode, otherObjectMessage);

			var validationMessage = new ValidationMessage(theObjectErrorCode, theObjectMessage);

			bool result = validationMessage.Equals(otherMessage);

			result.Should().BeFalse();
		}

		[Test]
		public void ItShouldNotBeEqualWhenErrorCodesDiffer()
		{
			string theObjectMessage = "message";
			string theObjectErrorCode = "error code";
			string otherErrorCode = "other error code";
			var otherMessage = new ValidationMessage(otherErrorCode, theObjectMessage);

			var validationMessage = new ValidationMessage(theObjectErrorCode, theObjectMessage);

			bool result = validationMessage.Equals(otherMessage);

			result.Should().BeFalse();
		}

		[Test]
		public void ItShouldNotThrowOnGeneratingHashCode()
		{
			var validationMessage = new ValidationMessage(null, null);

			Func<int> action = () => validationMessage.GetHashCode();

			action.Should().NotThrow();
		}
	}
}