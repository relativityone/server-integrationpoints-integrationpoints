using System;
using System.Collections;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Web.IntegrationPointsServices;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.IntegrationPointsServices
{
    [TestFixture, Category("Unit")]
    public class TextSanitizerTests
    {
        private Mock<IStringSanitizer> _stringSanitizerMock;
        private TextSanitizer _sut;

        [SetUp]
        public void SetUp()
        {
            _stringSanitizerMock = new Mock<IStringSanitizer>();

            _sut = new TextSanitizer(_stringSanitizerMock.Object);
        }

        [Test]
        public void Constructor_ShouldThrowExceptionWhenStringSanitizerIsNull()
        {
            // arrange
            IStringSanitizer internalSanitizer = null;

            // act
            Action constructor = () => new TextSanitizer(internalSanitizer);

            //
            constructor.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Sanitize_ShouldReturnNoErrorsWhenErrorsListIsEmpty()
        {
            // arrange
            var sanitizationResult = new SanitizeHtmlContentResult
            {
                ErrorMessages = new ArrayList()
            };
            SetupSanitizationResult(sanitizationResult);

            // act
            SanitizationResult result = _sut.Sanitize("text");

            // assert
            result.HasErrors.Should().BeFalse("because no errors were returned");
        }

        [Test]
        public void Sanitize_ShouldReturnNoErrorsWhenErrorsListIsNull()
        {
            // arrange
            var sanitizationResult = new SanitizeHtmlContentResult
            {
                ErrorMessages = null
            };
            SetupSanitizationResult(sanitizationResult);

            // act
            SanitizationResult result = _sut.Sanitize("text");

            // assert
            result.HasErrors.Should().BeFalse("because no errors were returned");
        }

        [TestCase("error")]
        [TestCase("")]
        [TestCase(null)]
        public void Sanitize_ShouldReturnResultWithErrorsWhenErrorsWerePresent(string error)
        {
            // arrange
            string[] errors = { error };
            var sanitizationResult = new SanitizeHtmlContentResult
            {
                ErrorMessages = new ArrayList(errors)
            };
            SetupSanitizationResult(sanitizationResult);

            // act
            SanitizationResult result = _sut.Sanitize("text");

            // assert
            result.HasErrors.Should().BeTrue("because errors were returned");
        }

        [TestCase("output")]
        [TestCase("cAseSenSiTive")]
        [TestCase("")]
        [TestCase(null)]
        public void Sanitize_ShouldReturnSanitizedText(string sanitizedText)
        {
            // arrange
            var sanitizationResult = new SanitizeHtmlContentResult
            {
                CleanHtml = sanitizedText
            };
            SetupSanitizationResult(sanitizationResult);

            // act
            SanitizationResult result = _sut.Sanitize("input");

            // assert
            result.SanitizedText.Should().Be(sanitizedText);
        }

        private void SetupSanitizationResult(SanitizeHtmlContentResult sanitizationResult)
        {
            _stringSanitizerMock
                .Setup(x => x.SanitizeHtmlContent(It.IsAny<string>()))
                .Returns(sanitizationResult);
        }
    }
}
