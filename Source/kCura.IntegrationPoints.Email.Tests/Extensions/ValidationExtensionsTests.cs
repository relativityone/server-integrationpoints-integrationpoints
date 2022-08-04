using FluentAssertions;
using kCura.IntegrationPoints.Email.Extensions;
using LanguageExt;
using NUnit.Framework;
using System;
using System.Linq;

namespace kCura.IntegrationPoints.Email.Tests.Extensions
{
    [TestFixture, Category("Unit")]
    public class ValidationExtensionsTests
    {
        private const int _VALID_VALUE = 7;
        private const string _ERROR_HEADER = "Error header";
        private readonly string[] errors = { "error1", "error2" };

        [Test]
        public void ToEither_ShouldReturnRightEitherForSuccessValidation()
        {
            // arrange
            Validation<string, int> successValidation = GetSuccessfulValidation();

            // act
            Either<string, int> either = successValidation.ToEither(errorHeader: _ERROR_HEADER);

            // assert
            either.IsRight.Should().BeTrue("because validation was successful");
        }

        [Test]
        public void ToEither_ShouldReturnLeftEitherForFailedValidaitonWithSingleError()
        {
            // arrange
            string validationError = errors.First();
            Validation<string, int> successValidation = GetFailedValidation(validationError);

            // act
            Either<string, int> either = successValidation.ToEither(errorHeader: _ERROR_HEADER);

            // assert
            string expectedError = $"{_ERROR_HEADER}. Errors: {validationError}";
            either.Match(
                Right: x => Assert.Fail("because it should be left when validation has failed"), 
                Left: error => error.Should().Be(expectedError)
            );
        }

        [Test]
        public void ToEither_ShouldReturnLeftEitherForFailedValidaitonWithTwoErrors()
        {
            // arrange
            string firstValidationError = errors.First();
            string secondValidationError = errors.First();
            Validation<string, int> successValidation = GetFailedValidation(firstValidationError, secondValidationError);

            // act
            Either<string, int> either = successValidation.ToEither(errorHeader: _ERROR_HEADER);

            // assert
            string expectedError = $"{_ERROR_HEADER}. Errors: {firstValidationError};{secondValidationError}";
            either.Match(
                Right: x => Assert.Fail("because it should be left when validation has failed"),
                Left: error => error.Should().Be(expectedError)
            );
        }

        private Validation<string, int> GetSuccessfulValidation()
        {
            return _VALID_VALUE;
        }

        private Validation<string, int> GetFailedValidation(string error1, string error2)
        {
            Validation<string, int> validation1 = GetFailedValidation(error1);
            Validation<string, int> validation2 = GetFailedValidation(error2);

            var validations = ValueTuple.Create(validation1, validation2);
            return validations.Apply((a, b) => a + b);
        }

        private Validation<string, int> GetFailedValidation(string error)
        {
            return error;
        }
    }
}
