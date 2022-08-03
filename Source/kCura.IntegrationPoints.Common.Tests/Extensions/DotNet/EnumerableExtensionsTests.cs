using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Common.Tests.Extensions.DotNet
{
    [TestFixture, Category("Unit")]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void IsNullOrEmpty_ShouldReturnTrueForNullEnumerable()
        {
            // arrange
            IEnumerable enumerable = null;

            // act
            bool isNullOrEmpty = enumerable.IsNullOrEmpty();

            // assert
            isNullOrEmpty.Should().BeTrue("because enumerable was null");
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnTrueForEmptyEnumerable()
        {
            // arrange
            IEnumerable enumerable = new ArrayList();

            // act
            bool isNullOrEmpty = enumerable.IsNullOrEmpty();

            // assert
            isNullOrEmpty.Should().BeTrue("because enumerable was empty");
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnFalseForNonEmptyEnumerable()
        {
            // arrange
            IEnumerable enumerable = new[] { string.Empty };

            // act
            bool isNullOrEmpty = enumerable.IsNullOrEmpty();

            // assert
            isNullOrEmpty.Should().BeFalse("because enumerable was non empty");
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnFalseForEnumerableWithSingleNull()
        {
            // arrange
            IEnumerable enumerable = new string[] { null };

            // act
            bool isNullOrEmpty = enumerable.IsNullOrEmpty();

            // assert
            isNullOrEmpty.Should().BeFalse("because enumerable was non empty");
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnTrueForNullGenericEnumerable()
        {
            // arrange
            IEnumerable<string> enumerable = null;

            // act
            bool isNullOrEmpty = enumerable.IsNullOrEmpty();

            // assert
            isNullOrEmpty.Should().BeTrue("because enumerable was null");
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnTrueForEmptyGenericEnumerable()
        {
            // arrange
            IEnumerable<string> enumerable = new List<string>();

            // act
            bool isNullOrEmpty = enumerable.IsNullOrEmpty();

            // assert
            isNullOrEmpty.Should().BeTrue("because enumerable was empty");
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnFalseForNonEmptyGenericEnumerable()
        {
            // arrange
            IEnumerable<string> enumerable = new[] { string.Empty };

            // act
            bool isNullOrEmpty = enumerable.IsNullOrEmpty();

            // assert
            isNullOrEmpty.Should().BeFalse("because enumerable was non empty");
        }

        [Test]
        public void IsNullOrEmpty_ShouldReturnFalseForGenericEnumerableWithSingleNull()
        {
            // arrange
            IEnumerable<string> enumerable = new string[] { null };

            // act
            bool isNullOrEmpty = enumerable.IsNullOrEmpty();

            // assert
            isNullOrEmpty.Should().BeFalse("because enumerable was non empty");
        }
    }
}
