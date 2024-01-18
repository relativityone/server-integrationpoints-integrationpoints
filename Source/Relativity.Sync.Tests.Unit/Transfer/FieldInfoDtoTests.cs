using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    internal sealed class FieldInfoDtoTests
    {
        [Test]
        public void ItShouldBuildGenericSpecialField()
        {
            // Arrange
            SpecialFieldType specialFieldType = SpecialFieldType.FolderPath;
            const string sourceFieldName = "Source Field";
            const string destinationFieldName = "Destination Field";

            // Act
            FieldInfoDto result = FieldInfoDto.GenericSpecialField(specialFieldType, sourceFieldName, destinationFieldName);

            // Assert
            result.SpecialFieldType.Should().Be(specialFieldType);
            result.SourceFieldName.Should().Be(sourceFieldName);
            result.DestinationFieldName.Should().Be(destinationFieldName);
            result.IsDocumentField.Should().BeFalse();
            result.IsIdentifier.Should().BeFalse();
        }

        [Test]
        public void ItShouldBuildDocumentField()
        {
            // Arrange
            const string sourceFieldName = "Source Field";
            const string destinationFieldName = "Destination Field";
            const bool isIdentifier = true;

            // Act
            FieldInfoDto result = FieldInfoDto.DocumentField(sourceFieldName, destinationFieldName, isIdentifier);

            // Assert
            result.SourceFieldName.Should().Be(sourceFieldName);
            result.DestinationFieldName.Should().Be(destinationFieldName);
            result.IsDocumentField.Should().BeTrue();
            result.IsIdentifier.Should().Be(isIdentifier);
        }

        [Test]
        public void ItShouldBuildFolderPathFieldForRetainingFolderStructure()
        {
            // Act
            FieldInfoDto result = FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure();

            // Assert
            result.SpecialFieldType.Should().Be(SpecialFieldType.FolderPath);
            result.SourceFieldName.Should().BeEmpty();
            result.DestinationFieldName.Should().Be("FolderPath_76B270CB-7CA9-4121-B9A1-BC0D655E5B2D");
            result.IsDocumentField.Should().BeFalse();
            result.IsIdentifier.Should().BeFalse();
        }

        [Test]
        public void ItShouldBuildFolderPathFieldForReadingFromField()
        {
            // Arrange
            const string fieldName = "Display Name Field";

            // Act
            FieldInfoDto result = FieldInfoDto.FolderPathFieldFromDocumentField(fieldName);

            // Assert
            result.SpecialFieldType.Should().Be(SpecialFieldType.FolderPath);
            result.SourceFieldName.Should().Be(fieldName);
            result.DestinationFieldName.Should().Be("FolderPath_76B270CB-7CA9-4121-B9A1-BC0D655E5B2D");
            result.IsDocumentField.Should().BeTrue();
            result.IsIdentifier.Should().BeFalse();
        }

        [Test]
        public void ItShouldBuildNativeFileFilenameField()
        {
            // Act
            FieldInfoDto result = FieldInfoDto.NativeFileFilenameField();

            // Assert
            result.SpecialFieldType.Should().Be(SpecialFieldType.NativeFileFilename);
            result.SourceFieldName.Should().BeEmpty();
            result.DestinationFieldName.Should().Be("NativeFileFilename");
            result.IsDocumentField.Should().BeFalse();
            result.IsIdentifier.Should().BeFalse();
        }

        [Test]
        public void ItShouldBuildNativeFileFileSizeField()
        {
            // Act
            FieldInfoDto result = FieldInfoDto.NativeFileSizeField();

            // Assert
            result.SpecialFieldType.Should().Be(SpecialFieldType.NativeFileSize);
            result.SourceFieldName.Should().BeEmpty();
            result.DestinationFieldName.Should().Be("NativeFileSize");
            result.IsDocumentField.Should().BeFalse();
            result.IsIdentifier.Should().BeFalse();
        }

        [Test]
        public void ItShouldBuildNativeFileLocationField()
        {
            // Act
            FieldInfoDto result = FieldInfoDto.NativeFileLocationField();

            // Assert
            result.SpecialFieldType.Should().Be(SpecialFieldType.NativeFileLocation);
            result.SourceFieldName.Should().BeEmpty();
            result.DestinationFieldName.Should().Be("NativeFileLocation");
            result.IsDocumentField.Should().BeFalse();
            result.IsIdentifier.Should().BeFalse();
        }

        [Test]
        public void ItShouldBuildSupportedByViewerField()
        {
            // Act
            FieldInfoDto result = FieldInfoDto.SupportedByViewerField();

            // Assert
            result.SpecialFieldType.Should().Be(SpecialFieldType.SupportedByViewer);
            result.SourceFieldName.Should().Be("SupportedByViewer");
            result.DestinationFieldName.Should().Be("SupportedByViewer");
            result.IsDocumentField.Should().BeTrue();
            result.IsIdentifier.Should().BeFalse();
        }

        [Test]
        public void ItShouldBuildRelativityNativeTypeField()
        {
            // Act
            FieldInfoDto result = FieldInfoDto.RelativityNativeTypeField();

            // Assert
            result.SpecialFieldType.Should().Be(SpecialFieldType.RelativityNativeType);
            result.SourceFieldName.Should().Be("RelativityNativeType");
            result.DestinationFieldName.Should().Be("RelativityNativeType");
            result.IsDocumentField.Should().BeTrue();
            result.IsIdentifier.Should().BeFalse();
        }

        [Test]
        public void ItShouldReturnEqualForTheSameObject()
        {
            // Arrange
            FieldInfoDto field1 = FieldInfoDto.DocumentField("source", "dest", false);
            FieldInfoDto field2 = field1;

            // Act
            bool equals = field1.Equals(field2);

            // Assert
            equals.Should().BeTrue();
        }

        [Test]
        public void ItShouldReturnEqualForObjectsWithSameProperties()
        {
            // Arrange
            FieldInfoDto field1 = FieldInfoDto.DocumentField("source", "dest", false);
            FieldInfoDto field2 = FieldInfoDto.DocumentField("source", "dest", false);

            // Act
            bool equals = field1.Equals(field2);

            // Assert
            equals.Should().BeTrue();
        }

        [Test]
        public void ItShouldReturnEqualForEquivalentObjectsCastToTypeObject()
        {
            // Arrange
            object field1 = FieldInfoDto.DocumentField("test", "test", false);
            object field2 = FieldInfoDto.DocumentField("test", "test", false);

            // Act
            bool equals = field1.Equals(field2);

            // Assert
            equals.Should().BeTrue();
        }

        private static IEnumerable<TestCaseData> EqualsDifferentPropertyCases()
        {
            // Each FieldInfoDto pair differ in only one property which is also the name of the test case.
            yield return new TestCaseData(
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "source", "dest"),
                FieldInfoDto.GenericSpecialField(SpecialFieldType.RelativityNativeType, "source", "dest"))
            {
                TestName = "SpecialFieldType"
            };

            yield return new TestCaseData(
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "foo", "foo"),
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "bar", "foo"))
            {
                TestName = "SourceFieldName"
            };

            yield return new TestCaseData(
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "foo", "foo"),
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "foo", "bar"))
            {
                TestName = "DestinationFieldName"
            };

            yield return new TestCaseData(
                FieldInfoDto.GenericSpecialField(SpecialFieldType.None, "test", "test"),
                FieldInfoDto.DocumentField("test", "test", false))
            {
                TestName = "IsDocumentField"
            };

            yield return new TestCaseData(
                FieldInfoDto.DocumentField("test", "test", false),
                FieldInfoDto.DocumentField("test", "test", true))
            {
                TestName = "IsIdentifier"
            };

            FieldInfoDto rdtField1 = FieldInfoDto.DocumentField("test", "test", false);
            rdtField1.RelativityDataType = RelativityDataType.Currency;
            FieldInfoDto rdtField2 = FieldInfoDto.DocumentField("test", "test", false);
            rdtField2.RelativityDataType = RelativityDataType.Date;
            yield return new TestCaseData(rdtField1, rdtField2)
            {
                TestName = "RelativityDataType"
            };

            FieldInfoDto dfiField1 = FieldInfoDto.DocumentField("test", "test", false);
            dfiField1.DocumentFieldIndex = 0;
            FieldInfoDto dfiField2 = FieldInfoDto.DocumentField("test", "test", false);
            dfiField2.DocumentFieldIndex = 1;
            yield return new TestCaseData(dfiField1, dfiField2)
            {
                TestName = "DocumentFieldIndex"
            };
        }

        [TestCaseSource(nameof(EqualsDifferentPropertyCases))]
        public void ItShouldReturnNotEqualForObjectsWithDifferentProperty(FieldInfoDto me, FieldInfoDto you)
        {
            // Act
            bool equals = me.Equals(you);

            // Assert
            equals.Should().BeFalse();
        }

        [Test]
        public void ItShouldReturnNotEqualForObjectOfDifferentType()
        {
            // Arrange
            FieldInfoDto me = FieldInfoDto.NativeFileFilenameField();
            object you = "test";

            // Act
            bool equals = me.Equals(you);

            // Assert
            equals.Should().BeFalse();
        }

        [Test]
        public void ItShouldReturnNotEqualWhenComparingToNull()
        {
            // Arrange
            FieldInfoDto me = FieldInfoDto.NativeFileFilenameField();
            FieldInfoDto you = null;

            // Act
            bool equals = me.Equals(you);

            // Assert
            equals.Should().BeFalse();
        }

        [Test]
        public void ItShouldReturnSameHashCodeForTheSameObject()
        {
            // Arrange
            FieldInfoDto field1 = FieldInfoDto.NativeFileLocationField();
            FieldInfoDto field2 = field1;

            // Act
            int hashCode1 = field1.GetHashCode();
            int hashCode2 = field2.GetHashCode();

            // Assert
            hashCode1.Should().Be(hashCode2);
        }

        [Test]
        public void ItShouldReturnSameHashCodeForDifferentButEqualObjects()
        {
            // Arrange
            FieldInfoDto field1 = FieldInfoDto.RelativityNativeTypeField();
            FieldInfoDto field2 = FieldInfoDto.RelativityNativeTypeField();

            // Act
            int hashCode1 = field1.GetHashCode();
            int hashCode2 = field2.GetHashCode();

            // Assert
            hashCode1.Should().Be(hashCode2);
        }

        [Test]
        public void ItShouldReturnSameHashCodeForTwoObjectsWithDifferentMutableValues()
        {
            // Arrange
            FieldInfoDto field1 = FieldInfoDto.NativeFileLocationField();
            field1.RelativityDataType = RelativityDataType.Currency;
            field1.DocumentFieldIndex = 0;
            FieldInfoDto field2 = FieldInfoDto.NativeFileLocationField();
            field2.RelativityDataType = RelativityDataType.File;
            field2.DocumentFieldIndex = 1;

            // Act
            int hashCode1 = field1.GetHashCode();
            int hashCode2 = field2.GetHashCode();

            // Assert
            hashCode1.Should().Be(hashCode2);
        }

        [Test]
        public void ItShouldReturnSameHashCodeForSameObjectAfterMutableUpdate()
        {
            // Arrange
            FieldInfoDto field = FieldInfoDto.NativeFileLocationField();

            // Act
            field.RelativityDataType = RelativityDataType.Currency;
            field.DocumentFieldIndex = 0;
            int firstHashCode = field.GetHashCode();

            field.RelativityDataType = RelativityDataType.Decimal;
            field.DocumentFieldIndex = 1;
            int secondHashCode = field.GetHashCode();

            // Assert
            firstHashCode.Should().Be(secondHashCode);
        }

        private static IEnumerable<TestCaseData> GetHashCodeDifferentPropertyCases()
        {
            // Each FieldInfoDto pair differ in only one immutable property which is also the name of the test case.
            yield return new TestCaseData(
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "test", "test"),
                FieldInfoDto.GenericSpecialField(SpecialFieldType.RelativityNativeType, "test", "test"))
            {
                TestName = "SpecialFieldType"
            };

            yield return new TestCaseData(
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "foo", "foo"),
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "bar", "foo"))
            {
                TestName = "SourceFieldName"
            };

            yield return new TestCaseData(
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "foo", "foo"),
                FieldInfoDto.GenericSpecialField(SpecialFieldType.FolderPath, "foo", "bar"))
            {
                TestName = "DestinationFieldName"
            };

            yield return new TestCaseData(
                FieldInfoDto.GenericSpecialField(SpecialFieldType.None, "test", "test"),
                FieldInfoDto.DocumentField("test", "test", false))
            {
                TestName = "IsDocumentField"
            };

            yield return new TestCaseData(
                FieldInfoDto.DocumentField("test", "test", false),
                FieldInfoDto.DocumentField("test", "test", true))
            {
                TestName = "IsIdentifier"
            };
        }

        [TestCaseSource(nameof(GetHashCodeDifferentPropertyCases))]
        public void ItShouldReturnDifferentHashCodeWithDifferentImmutableProperties(FieldInfoDto field1, FieldInfoDto field2)
        {
            // Arrange
            const int documentFieldIndex = 0;
            const RelativityDataType relativityDataType = RelativityDataType.Currency;
            field1.DocumentFieldIndex = documentFieldIndex;
            field2.DocumentFieldIndex = documentFieldIndex;
            field1.RelativityDataType = relativityDataType;
            field2.RelativityDataType = relativityDataType;

            // Act
            int hashCode1 = field1.GetHashCode();
            int hashCode2 = field2.GetHashCode();

            // Assert
            hashCode1.Should().NotBe(hashCode2);
        }
    }
}
