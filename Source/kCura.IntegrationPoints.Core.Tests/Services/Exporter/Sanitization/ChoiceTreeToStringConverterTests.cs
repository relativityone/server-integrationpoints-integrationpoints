using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
	[TestFixture]
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
			var choice = new ChoiceWithChildInfo(2, "Hot", Array.Empty<ChoiceWithChildInfo>());

			// act
			string actual = _sut.ConvertTreeToString(new List<ChoiceWithChildInfo> { choice });

			// assert
			string expected = $"Hot{_multiValueDelimiter}";
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
			string actual = _sut.ConvertTreeToString(new List<ChoiceWithChildInfo> { root });

			// assert
			string expected = $"Root{_nestedValueDelimiter}Child{_multiValueDelimiter}";
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ItShouldProperlyConvertMultipleParentsAndChildren()
		{
			/*
			 *		1
			 *			2
			 *				3
			 *			4
			 *		5
			 *			6
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

			string expected = $"1{_nestedValueDelimiter}2{_nestedValueDelimiter}3" +
				$"{_multiValueDelimiter}1{_nestedValueDelimiter}4" +
				$"{_multiValueDelimiter}5{_nestedValueDelimiter}6{_multiValueDelimiter}";

			// act
			string actual = _sut.ConvertTreeToString(new List<ChoiceWithChildInfo> { choice1, choice5 });

			// assert
			Assert.AreEqual(expected, actual);
		}
	}
}
