using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
	[TestFixture, Category("Unit")]
	public class DocumentFieldInfoTests
	{
		[TestCase("Decimal", "Decimal")]
		[TestCase("Yest/No", "Yest/No")]
		[TestCase("Single Object", "Single Object")]
		[TestCase("Object Identifier", "Object Identifier")]
		[TestCase("Long Text", "Long Text")]
		public void IsTypeCompatible_ShouldReturnTrue_WhenTypeAreSameButNoFixedLengthText(string sourceType, string destinationType)
		{
			// Arrange
			var sourceField = new FieldInfo("1", "Field1", sourceType);
			var destinationField = new FieldInfo("2", "Field2", destinationType);


			var result = sourceField.IsTypeCompatible(destinationField);

			// Assert
			result.Should().BeTrue();
		}

		[TestCase("Long Text", null)]
		[TestCase("Long Text", "")]
		[TestCase(null, "Long Text")]
		[TestCase("", "Long Text")]
		[TestCase(null, null)]
		[TestCase("", null)]
		[TestCase("", "")]
		[TestCase(null, "")]
		public void IsTypeCompatible_ShouldReturnTrue_WhenOneOfTheTypesIsNull(string sourceType, string destinationType)
		{
			// Arrange
			var sourceField = new FieldInfo("1", "Field1", sourceType);
			var destinationField = new FieldInfo("2", "Field2", destinationType);


			var result = sourceField.IsTypeCompatible(destinationField);

			// Assert
			result.Should().BeTrue();
		}

		[TestCase("Decimal", "Yest/No")]
		public void IsTypeCompatible_ShouldReturnFalse_WhenTypeAreDifferent(string sourceType, string destinationType)
		{
			// Arrange
			var sourceField = new FieldInfo("1", "Field1", sourceType);
			var destinationField = new FieldInfo("2", "Field2", destinationType);

			// Act
			var result = sourceField.IsTypeCompatible(destinationField);

			// Assert
			result.Should().BeFalse();
		}

		[TestCase("Fixed-Length Text(255)", "Long Text", true)]
		[TestCase("Fixed-Length Text(255)", "Fixed-Length Text(255)", true)]
		[TestCase("Fixed-Length Text(40)", "Fixed-Length Text(255)", true)]
		[TestCase("Fixed-Length Text(255)", "Fixed-Length Text(40)", false)]
		public void IsTypeCompatible_ShouldValidateFixedLengthText_WhenIsTypeIsExtended(string sourceType, string destinationType, bool expected)
		{
			// Arrange
			var sourceField = new FieldInfo("1", "Field1", sourceType);
			var destinationField = new FieldInfo("2", "Field2", destinationType);

			// Act
			var result = sourceField.IsTypeCompatible(destinationField);

			// Assert
			result.Should().Be(expected);
		}

		[TestCase("Fixed-Length Text", 255, "Long Text", 0, true)]
		[TestCase("Fixed-Length Text", 255, "Fixed-Length Text", 255, true)]
		[TestCase("Fixed-Length Text", 40, "Fixed-Length Text", 255, true)]
		[TestCase("Fixed-Length Text", 255, "Fixed-Length Text", 40, false)]
		public void IsTypeCompatible_ShouldValidateFixedLengthText_WhenLengthIsProvided(string sourceType, int sourceLength,
			string destinationType, int destinationLength, bool expected)
		{
			// Arrange
			var sourceField = new FieldInfo("1", "Field1", sourceType, sourceLength);
			var destinationField = new FieldInfo("2", "Field2", destinationType, destinationLength);

			// Act
			var result = sourceField.IsTypeCompatible(destinationField);

			// Assert
			result.Should().Be(expected);
		}

		[TestCase("Fixed-Length Text(255)", 0, "Fixed-Length Text", 255, true)]
		[TestCase("Fixed-Length Text(40)", 0, "Fixed-Length Text", 255, true)]
		[TestCase("Fixed-Length Text(255)", 0, "Fixed-Length Text", 40, false)]
		public void IsTypeCompatible_ShouldValidateFixedLengthText_WhenFixedLength(string sourceType, int sourceLength,
			string destinationType, int destinationLength, bool expected)
		{
			// Arrange
			var sourceField = new FieldInfo("1", "Field1", sourceType, sourceLength);
			var destinationField = new FieldInfo("2", "Field2", destinationType, destinationLength);

			// Act
			var result = sourceField.IsTypeCompatible(destinationField);

			// Assert
			result.Should().Be(expected);
		}

		[Test]
		public void DisplayType_ShouldReturnEmptyStringForNullType()
		{
			// Arrange 
			var field = new FieldInfo("1", "Name", null);

			// Act
			var displayType = field.DisplayType;

			// Assert
			displayType.Should().Be("");
		}
	}
}
