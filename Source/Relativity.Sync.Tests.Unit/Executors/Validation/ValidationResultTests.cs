using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class ValidationResultTests
	{
		[Test]
		public void ItShouldBeValidWhenConstructedWithNoParameters()
		{
			var validationResult = new ValidationResult();

			validationResult.IsValid.Should().BeTrue();
		}

		[Test]
		public void ItShouldAddValidationMessagesWhenConstructing()
		{
			const int expectedRecordCount = 2;
			var firstMessage = new ValidationMessage("first", "message");
			var secondMessage = new ValidationMessage("second", "message");

			ValidationResult validationResult = new ValidationResult(firstMessage, secondMessage);

			validationResult.IsValid.Should().BeFalse();
			validationResult.Messages.Should().HaveCount(expectedRecordCount);
			validationResult.Messages.Should().Contain(firstMessage);
			validationResult.Messages.Should().Contain(secondMessage);
		}

		[Test]
		public void ItShouldAddValidationMessage()
		{
			const int expectedRecordCount = 1;
			var message = new ValidationMessage("the", "message");
			ValidationResult validationResult = new ValidationResult();

			validationResult.Add(message);

			validationResult.IsValid.Should().BeFalse();
			validationResult.Messages.Should().HaveCount(expectedRecordCount);
			validationResult.Messages.Should().Contain(message);
		}

		[Test]
		public void ItShouldMergeTwoResults()
		{
			const int expectedRecordCount = 2;
			var messageFromOtherResult = new ValidationMessage("message from", "other result");
			var otherResult = new ValidationResult(messageFromOtherResult);

			var message = new ValidationMessage("the", "message");
			ValidationResult validationResult = new ValidationResult(message);

			validationResult.Add(otherResult);

			validationResult.IsValid.Should().BeFalse();
			validationResult.Messages.Should().HaveCount(expectedRecordCount);
			validationResult.Messages.Should().Contain(message);
			validationResult.Messages.Should().Contain(messageFromOtherResult);
		}

		[Test]
		public void ItShouldCreateAndAddNewValidationMessage()
		{
			const int expectedRecordCount = 1;
			const string theMessage = "The Message";
			ValidationResult validationResult = new ValidationResult();

			validationResult.Add(theMessage);

			validationResult.IsValid.Should().BeFalse();
			validationResult.Messages.Should().HaveCount(expectedRecordCount);
			validationResult.Messages.Should().Contain(validationMessage => validationMessage.ShortMessage == theMessage);
		}

		[Test]
		public void ItShouldReturnMessageInToString()
		{
			var firstMessage = new ValidationMessage("first", "message");
			var secondMessage = new ValidationMessage("second", "message");

			ValidationResult validationResult = new ValidationResult(firstMessage, secondMessage);

			string result = validationResult.ToString();

			result.Should().Be($"first message{System.Environment.NewLine}second message");
		}
	}
}
