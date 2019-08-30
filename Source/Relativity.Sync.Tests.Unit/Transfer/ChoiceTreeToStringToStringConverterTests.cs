using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal sealed class ChoiceTreeToStringToStringConverterTests
	{
#pragma warning disable RG2009 // Hardcoded Numeric Value

		private ConfigurationStub _config;
		private ChoiceTreeToStringConverter _instance;

		[SetUp]
		public void SetUp()
		{
			_config = new ConfigurationStub();
			_instance = new ChoiceTreeToStringConverter(_config);
		}

		[Test]
		public void ItShouldProperlyConvertOneRootChoice()
		{
			var choice = new ChoiceWithChildInfo(2, "Hot", Array.Empty<ChoiceWithChildInfo>());

			// act
			string actual = _instance.ConvertTreeToString(new List<ChoiceWithChildInfo> { choice });

			// assert
			string expected = $"Hot{_config.MultiValueDelimiter}";
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
			string expected = $"Root{_config.NestedValueDelimiter}Child{_config.MultiValueDelimiter}";
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

			string expected = $"1{_config.NestedValueDelimiter}2{_config.NestedValueDelimiter}3" +
				$"{_config.MultiValueDelimiter}1{_config.NestedValueDelimiter}4" +
				$"{_config.MultiValueDelimiter}5{_config.NestedValueDelimiter}6{_config.MultiValueDelimiter}";

			// act
			string actual = _instance.ConvertTreeToString(new List<ChoiceWithChildInfo> { choice1, choice5 });

			// assert
			Assert.AreEqual(expected, actual);
		}

#pragma warning restore RG2009 // Hardcoded Numeric Value
	}
}