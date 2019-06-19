using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal sealed class ChoiceTreeToStringToStringConverterTests
	{
#pragma warning disable RG2009 // Hardcoded Numeric Value

		private Mock<ISynchronizationConfiguration> _config;
		private ChoiceTreeToStringConverter _instance;

		private const char _MULTI_VALUE_DELIMITER = ';';
		private const char _NESTED_VALUE_DELIMITER = '/';
		
		[SetUp]
		public void SetUp()
		{
			_config = new Mock<ISynchronizationConfiguration>();
			ImportSettingsDto importSettings = new ImportSettingsDto()
			{
				MultiValueDelimiter = _MULTI_VALUE_DELIMITER,
				NestedValueDelimiter = _NESTED_VALUE_DELIMITER
			};
			_config.SetupGet(x => x.ImportSettings).Returns(importSettings);
			_instance = new ChoiceTreeToStringConverter(_config.Object);
		}

		[Test]
		public void ItShouldProperlyConvertOneRootChoice()
		{
			ChoiceWithParentInfo choice = new ChoiceWithParentInfo(-1, 2, "Hot");

			// act
			string actual = _instance.ConvertTreeToString(new List<ChoiceWithParentInfo>() {choice});

			// assert
			string expected = $"Hot;";
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ItShouldProperlyConvertRootAndChild()
		{
			const string parentName = "Root";
			const int parentArtifactId = 2;
			ChoiceWithParentInfo root = new ChoiceWithParentInfo(-1, parentArtifactId, parentName);
			const string childName = "Child";
			ChoiceWithParentInfo child = new ChoiceWithParentInfo(parentArtifactId, 1, childName);
			root.Children.Add(child);

			// act
			string actual = _instance.ConvertTreeToString(new List<ChoiceWithParentInfo>() { root });

			// assert
			string expected = $"Root/Child;";
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
			ChoiceWithParentInfo choice1 = new ChoiceWithParentInfo(0, 0, "1");
			ChoiceWithParentInfo choice2 = new ChoiceWithParentInfo(0, 0, "2");
			ChoiceWithParentInfo choice3 = new ChoiceWithParentInfo(0, 0, "3");
			ChoiceWithParentInfo choice4 = new ChoiceWithParentInfo(0, 0, "4");
			ChoiceWithParentInfo choice5 = new ChoiceWithParentInfo(0, 0, "5");
			ChoiceWithParentInfo choice6 = new ChoiceWithParentInfo(0, 0, "6");

			choice1.Children.Add(choice2);
			choice1.Children.Add(choice4);
			choice2.Children.Add(choice3);
			choice5.Children.Add(choice6);

			string expected = $"1/2/3;1/4;5/6;";

			// act
			string actual = _instance.ConvertTreeToString(new List<ChoiceWithParentInfo>() { choice1, choice5 });

			// assert
			Assert.AreEqual(expected, actual);
		}
		
#pragma warning restore RG2009 // Hardcoded Numeric Value
	}
}