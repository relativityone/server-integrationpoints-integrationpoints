using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
    [TestFixture, Category("Unit")]
    internal sealed class ChoiceTreeToStringConverterTests
    {
        private ChoiceTreeToStringConverter _sut;
        private char _multiValueDelimiter;
        private char _nestedValueDelimiter;

        [SetUp]
        public void SetUp()
        {
            _sut = new ChoiceTreeToStringConverter();
            _multiValueDelimiter = IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER;
            _nestedValueDelimiter = IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER;
        }

        [Test]
        public void ItShouldProperlyConvertOneRootChoice()
        {
            // arrange
            var choice = new ChoiceWithChildInfoDto(2, "Hot", Array.Empty<ChoiceWithChildInfoDto>());

            // act
            string actual = _sut.ConvertTreeToString(new List<ChoiceWithChildInfoDto> { choice });

            // assert
            string expected = $"Hot{_multiValueDelimiter}";
            actual.Should().Be(expected);
        }

        [Test]
        public void ItShouldProperlyConvertRootAndChild()
        {
            // arrange
            const string childName = "Child";
            const int childArtifactId = 103556;
            var child = new ChoiceWithChildInfoDto(childArtifactId, childName, Array.Empty<ChoiceWithChildInfoDto>());

            const string parentName = "Root";
            const int parentArtifactId = 104334;
            var root = new ChoiceWithChildInfoDto(parentArtifactId, parentName, new List<ChoiceWithChildInfoDto> { child });

            // act
            string actual = _sut.ConvertTreeToString(new List<ChoiceWithChildInfoDto> { root });

            // assert
            string expected = $"Root{_nestedValueDelimiter}Child{_multiValueDelimiter}";
            actual.Should().Be(expected);
        }

        [Test]
        public void ItShouldProperlyConvertMultipleParentsAndChildren()
        {
            // arrange
            /*
             *        1
             *            2
             *                3
             *            4
             *        5
             *            6
             */
            var choice1 = new ChoiceWithChildInfoDto(0, "1", new List<ChoiceWithChildInfoDto>());
            var choice2 = new ChoiceWithChildInfoDto(0, "2", new List<ChoiceWithChildInfoDto>());
            var choice3 = new ChoiceWithChildInfoDto(0, "3", new List<ChoiceWithChildInfoDto>());
            var choice4 = new ChoiceWithChildInfoDto(0, "4", new List<ChoiceWithChildInfoDto>());
            var choice5 = new ChoiceWithChildInfoDto(0, "5", new List<ChoiceWithChildInfoDto>());
            var choice6 = new ChoiceWithChildInfoDto(0, "6", new List<ChoiceWithChildInfoDto>());

            choice1.Children.Add(choice2);
            choice1.Children.Add(choice4);
            choice2.Children.Add(choice3);
            choice5.Children.Add(choice6);

            string expected = $"1{_nestedValueDelimiter}2{_nestedValueDelimiter}3" +
                $"{_multiValueDelimiter}1{_nestedValueDelimiter}4" +
                $"{_multiValueDelimiter}5{_nestedValueDelimiter}6{_multiValueDelimiter}";

            // act
            string actual = _sut.ConvertTreeToString(new List<ChoiceWithChildInfoDto> { choice1, choice5 });

            // assert
            actual.Should().Be(expected);
        }
    }
}
