using System;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Unit
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
            T actualResult = testDescription.GetEnumFromDescription<T>();

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void GetEnumFromDescriptionThrowsWhenNotEnumTypeTest()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => string.Empty.GetEnumFromDescription<Type>(),
                $"The type specified is not an enum type: {nameof(Type)}.");
        }

        [Test]
        public void GetEnumFromDescriptionThrowsWhenNoDescriptionsExistTest()
        {
            // Arrange
            string testDescription = "Append Only";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => testDescription.GetEnumFromDescription<ImportOverwriteMode>(),
                $"The description could not be converted to the proper enum value: {testDescription}.");
        }

        [Test]
        public void GetEnumFromDescriptionThrowsWhenNoDescriptionFoundTest()
        {
            // Arrange
            string testDescription = string.Empty;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => testDescription.GetEnumFromDescription<FieldOverlayBehavior>(),
                $"The description could not be converted to the proper enum value: {testDescription}.");
        }
    }
}
