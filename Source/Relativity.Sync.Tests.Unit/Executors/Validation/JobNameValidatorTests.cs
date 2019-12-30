﻿using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class JobNameValidatorTests
	{
		private CancellationToken _cancellationToken;
		private JobNameValidator _instance;

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;
			_instance = new JobNameValidator(new EmptyLogger());
		}

		[Test]
		[TestCase("GoodJobName", ExpectedResult = true)]
		[TestCase("BadJob<Name", ExpectedResult = false)]
		[TestCase("BadJob\\Name", ExpectedResult = false)]
		[TestCase("BadJob?Name", ExpectedResult = false)]
		[TestCase("BadJob\tName", ExpectedResult = false)]
		[TestCase("BadJob|Name", ExpectedResult = false)]
		[TestCase("BadJob:Name", ExpectedResult = false)]
		[TestCase("BadJob*Name", ExpectedResult = false)]
		[TestCase("", ExpectedResult = false)]
		public async Task<bool> ValidateAsyncTests(string testJobName)
		{
			// Arrange
			var validationConfiguration = new Mock<IValidationConfiguration>();
			validationConfiguration.Setup(x => x.GetJobName()).Returns(testJobName).Verifiable();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Mock.VerifyAll(validationConfiguration);
			return actualResult.IsValid;
		}
	}
}