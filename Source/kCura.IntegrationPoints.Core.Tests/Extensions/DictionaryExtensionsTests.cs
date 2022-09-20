using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Domain.Exceptions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Extensions
{
    [TestFixture, Category("Unit")]
    public class DictionaryExtensionsTests
    {
        [Test]
        public void AddOrThrowIfKeyExists_ShouldAddToDictionaryWhenKeyDoesNotExist()
        {
            //arrange
            var dictionary = new Dictionary<int, int>();
            const int expectedKey = 1;
            const int expectedValue = 10;

            //act
            dictionary.AddOrThrowIfKeyExists(
                expectedKey,
                expectedValue,
                errorMessage: "Some error message"
            );

            //assert
            dictionary[expectedKey].Should().Be(expectedValue);
        }

        [Test]
        public void AddOrThrowIfKeyExists_ShouldThrowWhenKeyExists()
        {
            //arrange
            const int expectedKey = 1;
            const int expectedValue = 10;
            const string errorMessage = "Some error message";
            var dictionary = new Dictionary<int, int>
            {
                [expectedKey] = expectedValue
            };

            //act
            Action action = () => dictionary.AddOrThrowIfKeyExists(
                expectedKey,
                expectedValue,
                errorMessage
            );

            //assert
            action
                .ShouldThrow<IntegrationPointsException>()
                .WithMessage(
                    $"{errorMessage}, key: {expectedKey}, value: {expectedValue}"
                );
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddOrThrowIfKeyExists_ShouldThrowWhenErrorMessageIsNullOrEmpty(string errorMessage)
        {
            //arrange
            var dictionary = new Dictionary<int, int>();
            const int expectedKey = 1;
            const int expectedValue = 10;

            //act
            Action action = () => dictionary.AddOrThrowIfKeyExists(
                expectedKey,
                expectedValue,
                errorMessage
            );

            //assert
            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void AddDictionary_ShouldAddWhenKeyDoesNotExists()
        {
            // ARRANGE
            var startingDictionary = new Dictionary<int, int>()
            {
                { 1, 1 }
            };

            var inputDictionary = new Dictionary<int, int>()
            {
                { 20, 20 }
            };

            // ACT
            startingDictionary.AddDictionary(inputDictionary);

            // ASSERT
            startingDictionary.Should().Contain(inputDictionary);
            startingDictionary.Should().HaveCount(2);
        }

        [Test]
        public void AddDictionary_ShouldNotAddWhenKeyDoesExists()
        {
            // ARRANGE
            var startingDictionary = new Dictionary<int, int>()
            {
                { 1, 1 }
            };

            var inputDictionary = new Dictionary<int, int>()
            {
                { 1, 1 }
            };

            // ACT
            Action action = () => startingDictionary.AddDictionary(inputDictionary);

            // ASSERT
            action.ShouldThrow<IntegrationPointsException>();
        }
    }
}
