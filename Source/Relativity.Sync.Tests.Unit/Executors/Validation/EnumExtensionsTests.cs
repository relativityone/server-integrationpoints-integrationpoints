using System;
using NUnit.Framework;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public class EnumExtensionsTests
	{
		[Test]
		[TestCase(FieldOverlayBehavior.ReplaceValues, "Replace Values")]
		[TestCase(ImportOverwriteMode.AppendOverlay, "AppendOverlay")]
		public void GetEnumFromDescriptionGoldFlowTests<T>(T expectedResult, string testDescription)
		{
			// Act
			T actualResult = EnumExtensions.GetEnumFromDescription<T>(testDescription);

			// Assert
			Assert.AreEqual(expectedResult, actualResult);
		}

		[Test]
		public void GetEnumFromDescriptionThrowsWhenNotEnumTypeTest()
		{
			// Act & Assert
			Assert.Throws<InvalidOperationException>(() => EnumExtensions.GetEnumFromDescription<Type>(string.Empty),
				$"The type specified is not an enum type: {nameof(Type)}.");
		}

		[Test]
		public void GetEnumFromDescriptionThrowsWhenNoDescriptionsExistTest()
		{
			// Arrange
			string testDescription = "Append Only";

			// Act & Assert
			Assert.Throws<InvalidOperationException>(() => EnumExtensions.GetEnumFromDescription<ImportOverwriteMode>(testDescription),
				$"The description could not be converted to the proper enum value: {testDescription}.");
		}

		[Test]
		public void GetEnumFromDescriptionThrowsWhenNoDescriptionFoundTest()
		{
			// Arrange
			string testDescription = string.Empty;

			// Act & Assert
			Assert.Throws<InvalidOperationException>(() => EnumExtensions.GetEnumFromDescription<FieldOverlayBehavior>(testDescription),
				$"The description could not be converted to the proper enum value: {testDescription}.");
		}
	}
}