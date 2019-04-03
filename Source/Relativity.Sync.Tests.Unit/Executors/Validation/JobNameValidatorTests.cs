using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class JobNameValidatorTests
	{
		private CancellationToken _cancellationToken;

		private Mock<ISyncLog> _syncLog;

		private JobNameValidator _instance;

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = CancellationToken.None;

			_syncLog = new Mock<ISyncLog>();

			_instance = new JobNameValidator(_syncLog.Object);
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
			validationConfiguration.SetupGet(x => x.JobName).Returns(testJobName).Verifiable();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(validationConfiguration.Object, _cancellationToken).ConfigureAwait(false);

			// Assert
			Mock.VerifyAll(validationConfiguration);
			return actualResult.IsValid;
		}
	}
}