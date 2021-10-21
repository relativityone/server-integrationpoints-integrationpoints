using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common.Attributes;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class JobNameValidatorTests
	{
		private CancellationToken _cancellationToken;
		private JobNameValidator _sut;

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;
			_sut = new JobNameValidator(new EmptyLogger());
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
		public async Task<bool> ValidateAsync_ShouldHandleTestCases(string testJobName)
		{
			// Arrange
			var validationConfiguration = new Mock<IValidationConfiguration>();
			validationConfiguration.Setup(x => x.GetJobName()).Returns(testJobName).Verifiable();

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Mock.VerifyAll(validationConfiguration);
			return actualResult.IsValid;
		}

		[TestCase(typeof(SyncDocumentRunPipeline), true)]
		[TestCase(typeof(SyncDocumentRetryPipeline), true)]
		[TestCase(typeof(SyncImageRunPipeline), true)]
		[TestCase(typeof(SyncImageRetryPipeline), true)]
		[TestCase(typeof(SyncNonDocumentRunPipeline), true)]
		[EnsureAllPipelineTestCase(0)]
		public void ShouldExecute_ShouldReturnCorrectValue(Type pipelineType, bool expectedResult)
		{
			// Arrange
			ISyncPipeline pipelineObject = (ISyncPipeline)Activator.CreateInstance(pipelineType);

			// Act
			bool actualResult = _sut.ShouldValidate(pipelineObject);

			// Assert
			actualResult.Should().Be(expectedResult,
				$"ShouldValidate should return {expectedResult} for pipeline {pipelineType.Name}");
		}
	}
}