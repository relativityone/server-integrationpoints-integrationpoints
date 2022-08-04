using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
    [TestFixture, Category("Unit")]
    public class FileCountValidatorTests
    {
        [TestCase(-1)]
        [TestCase(0)]
        public void ItShouldFailValidationForInvalidCount(int count)
        {
            // arrange
            var validator = new FileCountValidator();

            // act
            var actual = validator.Validate(count);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.That(actual.MessageTexts.FirstOrDefault(), Is.Not.Null.Or.Empty);
        }

        [TestCase(42)]
        public void ItShouldPassForValidCount(int count)
        {
            // arrange
            var validator = new FileCountValidator();

            // act
            var actual = validator.Validate(count);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.FirstOrDefault(), Is.Null);
        }
    }
}