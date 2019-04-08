using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class EmailValidatorTests
	{
		private CancellationToken _cancellationToken;

		private Mock<ISyncLog> _syncLog;

		private EmailValidator _instance;

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;

			_syncLog = new Mock<ISyncLog>();

			_instance = new EmailValidator(_syncLog.Object);
		}

		[Test]
		[TestCase("")]
		[TestCase("hello@world.com")]
		[TestCase("hello@world.com;;relativity.admin@kcura.com")]
		[TestCase("hello@world.com;relativity.admin@kcura.onmicrosoft.com;john_doe@gmail.com")]
		[TestCase("disposable.style.email.with+symbol@example.com")]
		[TestCase("other.email-with-hyphen@example.com")]
		[TestCase("x@example.com")]
		[TestCase("example@s.example")]
		[TestCase("\" \"@example.org")]
		[TestCase("\"john..doe\"@example.org")]
		[TestCase("admin@mailserver1", Ignore = "We do not allow dot-less email addresses.")]
		public async Task ValidateAsyncValidEmailsTests(string testEmails)
		{
			// Arrange
			var validationConfiguration = new Mock<IValidationConfiguration>();
			validationConfiguration.SetupGet(x => x.NotificationEmails).Returns(testEmails).Verifiable();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsTrue(actualResult.IsValid);

			Mock.VerifyAll(validationConfiguration);
		}

		[Test]
		[TestCase("hello.world.com")]
		[TestCase("hello@@world.com")]
		[TestCase("hello@world.com; ;relativity.admin@kcura.com")]
		[TestCase("a\"b(c)d,e:f;g<h>i[j\\k]l@example.com")]
		[TestCase("just\"not\"right@example.com")]
		[TestCase("this is\"not\\allowed@example.com")]
		[TestCase("this\\ still\\\"not\\\\allowed@example.com")]
		[TestCase("1234567890123456789012345678901234567890123456789012345678901234+x@example.com", Ignore = "We do not check for long local addresses.")]
		public async Task ValidateAsyncInvalidEmailsTests(string testEmails)
		{
			// Arrange
			var validationConfiguration = new Mock<IValidationConfiguration>();
			validationConfiguration.SetupGet(x => x.NotificationEmails).Returns(testEmails).Verifiable();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Assert.IsFalse(actualResult.IsValid);
			Assert.IsNotEmpty(actualResult.Messages);

			Mock.VerifyAll(validationConfiguration);
			_syncLog.Verify(x => x.LogError(It.IsAny<string>()), Times.AtLeastOnce());
		}
	}
}