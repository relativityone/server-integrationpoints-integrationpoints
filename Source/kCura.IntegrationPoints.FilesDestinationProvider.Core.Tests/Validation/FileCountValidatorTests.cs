using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation
{
	public class FileCountValidatorTests
	{
		private FileCountValidator _fileCountValidator;

		[SetUp]
		public void SetUp()
		{
			_fileCountValidator = new FileCountValidator();
		}

		[Test]
		public void ItShouldReturnWarningWhenTotalDocCountIsZero()
		{
			var result = _fileCountValidator.Validate(0);

			Assert.That(result.IsValid, Is.False);
			Assert.That(result.Messages.FirstOrDefault(), Is.EqualTo("There are no items to export. Verify your source location."));
		}

		[Test]
		[TestCase(1)]
		[TestCase(10)]
		[TestCase(1000000)]
		public void ItShouldNotReturnWarningWhenTotalDocCountIsGreaterThanZero(int totalDocCount)
		{
			var result = _fileCountValidator.Validate(totalDocCount);

			Assert.That(result.IsValid, Is.True);			
			Assert.That(result.Messages.FirstOrDefault(), Is.Null.Or.Empty);
		}
	}
}