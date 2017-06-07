using NUnit.Framework;
using System;
using System.Data;
using FluentAssertions;

namespace kCura.IntegrationPoints.Domain.Tests
{
    [TestFixture]
    public class UtilsTests
    {
        private const string _OUTER_EXCEPTION_MESSAGE = "outer exception message";
        private const string _INNER_EXCEPTION_MESSAGE = "inner exception message";

        [Test]
        public void GetNonCustomExceptionTest()
        {
            // Arrange
            Exception exception = Utils.GetNonCustomException(CreateException());

            // Act / Assert
            AssertStringRepresentationOfException(Utils.GetPrintableException(exception));
        }

        [Test]
        public void GetPrintableExceptionTest()
        {
            // Arrange
            Exception exception = CreateException();

            // Act / Assert
            AssertStringRepresentationOfException(Utils.GetPrintableException(exception));
        }

        private void AssertStringRepresentationOfException(string stringRepresentation)
        {
            stringRepresentation.Should().StartWith(_OUTER_EXCEPTION_MESSAGE);
            stringRepresentation.Should().Contain("at " + GetType().FullName);
            stringRepresentation.Should().Contain(_INNER_EXCEPTION_MESSAGE);
        }

        private Exception CreateException()
        {
            try
            {
                throw new IndexOutOfRangeException(_INNER_EXCEPTION_MESSAGE);
            }
            catch (Exception innerException)
            {
                try
                {
                    throw new DataException(_OUTER_EXCEPTION_MESSAGE, innerException);
                }
                catch (Exception outerException)
                {
                    return outerException;
                }
            }
        }

        [Test]
        [TestCase("name string", -4, "name string - -4")]
        [TestCase("name string", null, "name string")]
        [TestCase(null, 3, " - 3")]
        public void GetFormatForWorkspaceOrJobDisplayReturnsProperString(string name, int? id, string result)
        {
            // Act / Assert
            Utils.GetFormatForWorkspaceOrJobDisplay(name, id).Should().Be(result);
        }

        [Test]
        public void GetFormatForWorkspaceOrJobDisplayReturnsProperStringForPrefix()
        {
            // Act / Assert
            Utils.GetFormatForWorkspaceOrJobDisplay("some prefix", "name string", -3)
                .Should()
                .Be("some prefix - name string - -3");
        }
    }
}