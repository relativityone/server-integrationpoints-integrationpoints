using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal sealed class ChoiceTreeToStringConverterTests
    {
#pragma warning disable RG2009 // Hardcoded Numeric Value
        private Mock<IDocumentSynchronizationConfiguration> _config;
        private ChoiceTreeToStringConverter _instance;

        private const char _NESTED_VALUE_DELIMITER = (char)29;
        private const char _MULTI_VALUE_DELIMITER = (char)30;

        [SetUp]
        public void SetUp()
        {
            _config = new Mock<IDocumentSynchronizationConfiguration>();
            _config.SetupGet(x => x.NestedValueDelimiter).Returns(_NESTED_VALUE_DELIMITER);
            _config.SetupGet(x => x.MultiValueDelimiter).Returns(_MULTI_VALUE_DELIMITER);
            _instance = new ChoiceTreeToStringConverter(_config.Object);
        }

        [Test]
        public void ItShouldProperlyConvertOneRootChoice()
        {
            var choice = new ChoiceWithChildInfo(2, "Hot", Array.Empty<ChoiceWithChildInfo>());

            // act
            string actual = _instance.ConvertTreeToString(new List<ChoiceWithChildInfo> { choice });

            // assert
            string expected = $"Hot{_MULTI_VALUE_DELIMITER}";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ItShouldProperlyConvertRootAndChild()
        {
            const string childName = "Child";
            const int childArtifactId = 103556;
            var child = new ChoiceWithChildInfo(childArtifactId, childName, Array.Empty<ChoiceWithChildInfo>());

            const string parentName = "Root";
            const int parentArtifactId = 104334;
            var root = new ChoiceWithChildInfo(parentArtifactId, parentName, new List<ChoiceWithChildInfo> { child });

            // act
            string actual = _instance.ConvertTreeToString(new List<ChoiceWithChildInfo> { root });

            // assert
            string expected = $"Root{_NESTED_VALUE_DELIMITER}Child{_MULTI_VALUE_DELIMITER}";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ItShouldProperlyConvertMultipleParentsAndChildren()
        {
            /*
             *        1
             *            2
             *                3
             *            4
             *        5
             *            6
             */
            var choice1 = new ChoiceWithChildInfo(0, "1", new List<ChoiceWithChildInfo>());
            var choice2 = new ChoiceWithChildInfo(0, "2", new List<ChoiceWithChildInfo>());
            var choice3 = new ChoiceWithChildInfo(0, "3", new List<ChoiceWithChildInfo>());
            var choice4 = new ChoiceWithChildInfo(0, "4", new List<ChoiceWithChildInfo>());
            var choice5 = new ChoiceWithChildInfo(0, "5", new List<ChoiceWithChildInfo>());
            var choice6 = new ChoiceWithChildInfo(0, "6", new List<ChoiceWithChildInfo>());

            choice1.Children.Add(choice2);
            choice1.Children.Add(choice4);
            choice2.Children.Add(choice3);
            choice5.Children.Add(choice6);

            string expected = $"1{_NESTED_VALUE_DELIMITER}2{_NESTED_VALUE_DELIMITER}3" +
                $"{_MULTI_VALUE_DELIMITER}1{_NESTED_VALUE_DELIMITER}4" +
                $"{_MULTI_VALUE_DELIMITER}5{_NESTED_VALUE_DELIMITER}6{_MULTI_VALUE_DELIMITER}";

            // act
            string actual = _instance.ConvertTreeToString(new List<ChoiceWithChildInfo> { choice1, choice5 });

            // assert
            actual.Should().Be(expected);
        }
#pragma warning restore RG2009 // Hardcoded Numeric Value
    }
}
