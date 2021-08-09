using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal class ImageInfoRowValuesBuilderTests
	{
		private ImageInfoRowValuesBuilder _sut;

		[Test]
		public void BuildRowValues_ShouldReturnEmpty_WhenDocumentDoesNotExistInImageFiles()
		{
			// Arrange
			const int documentId = 1;
			const int nonExsitingDocumentId = 2;

			var DocumentToImageFiles = new Dictionary<int, ImageFile[]>()
			{
				{ documentId, new[] { new ImageFile(documentId, "1","Location1", "Name1", 0) } }
			};

			var notExistingDocument = new RelativityObjectSlim { ArtifactID = nonExsitingDocumentId };

			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);

			// Act
			var result = _sut.BuildRowsValues(It.IsAny<FieldInfoDto>(), notExistingDocument, _ => "");

			// Assert
			result.Should().BeEmpty();
		}

		[Test]
		public void BuildRowValues_ShouldReturnEmpty_WhenDocumentHasNotAnyImages()
		{
			// Arrange
			const int documentId = 1;
			const int documentWithouImagesId = 2;

			var DocumentToImageFiles = new Dictionary<int, ImageFile[]>()
			{
				{ documentId, new[] { new ImageFile(documentId, "1", "Location1", "Name1", 0) } },
				{ documentWithouImagesId, new ImageFile[] { } }
			};

			var documentWithoutImages = new RelativityObjectSlim { ArtifactID = documentWithouImagesId };

			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);

			// Act
			var result = _sut.BuildRowsValues(It.IsAny<FieldInfoDto>(), documentWithoutImages, _ => "");

			// Assert
			result.Should().BeEmpty();
		}

		public static IEnumerable<TestCaseData> SpecialFieldExpectedReturnValuesData
			=> new[]
				{
					new TestCaseData(FieldInfoDto.ImageFileNameField(), new object[] { "Name2a", "Name2b", "Name2c" } ),
					new TestCaseData(FieldInfoDto.ImageFileLocationField(), new object[] { "Location2a", "Location2b", "Location2c" } )
				};

		[TestCaseSource(nameof(SpecialFieldExpectedReturnValuesData))]
		public void BuildRowValues_ShouldValues_WhenSpecialFieldTypeHasBeenProvided(FieldInfoDto specialField, IEnumerable<object> expectedValues)
		{
			// Arrange
			var DocumentToImageFiles = new Dictionary<int, ImageFile[]>()
			{
				{ 1, new[] { new ImageFile(1, "1","Location1", "Name1", 0) } },
				{ 2, new[] { new ImageFile(2, "2a","Location2a", "Name2a", 0), new ImageFile(2, "2b","Location2b", "Name2b", 0), new ImageFile(2, "2c", "Location2c", "Name2c", 0) } },
				{ 3, new[] { new ImageFile(3, "3","Location3", "Name3", 0) } }
			};

			var document = new RelativityObjectSlim { ArtifactID = 2 };

			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);

			// Act
			var result = _sut.BuildRowsValues(specialField, document, _ => "");

			// Assert
			result.Should().BeEquivalentTo(expectedValues);
		}

		[Test]
		public void BuildRowValues_ShouldThrow_WhenSpecialFieldTypeIsNotAllowed()
		{
			// Arrange
			const int documentId = 1;

			var DocumentToImageFiles = new Dictionary<int, ImageFile[]>()
			{
				{ documentId, new[] { new ImageFile(documentId, "1","Location1", "Name1", 0) } }
			};

			var field = FieldInfoDto.NativeFileLocationField();

			var document = new RelativityObjectSlim { ArtifactID = documentId };

			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);

			// Act
			Func<object> action = () => _sut.BuildRowsValues(field, document, _ => "");

			// Assert
			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public void BuildRowValues_Should_GenerateCorrectIdentifier()
		{
			// Arrange
			const int documentId = 1;

			var DocumentToImageFiles = new Dictionary<int, ImageFile[]>()
			{
				{ documentId, Enumerable.Range(1, 1001).Select(x => new ImageFile(documentId, $"identifier_{x}", $"location_{x}", $"filename_{x}", 5)).ToArray() }
			};

			var field = FieldInfoDto.ImageIdentifierField();

			var document = new RelativityObjectSlim { ArtifactID = documentId };
			
			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);
			
			// Act
			string controlNumber = "document";
			string[] result = _sut.BuildRowsValues(field, document, _ => controlNumber).Select(x => x.ToString()).ToArray();

			// Assert
			result.All(x => x.StartsWith(controlNumber)).Should().BeTrue("All images identifiers should start with control number");

			result.First().Should().Be(controlNumber, "First image identifier should be just control number");

			AssertIdentifierAt(result, 5, controlNumber + "_0005");
			AssertIdentifierAt(result, 50, controlNumber + "_0050");
			AssertIdentifierAt(result, 500, controlNumber + "_0500");
			AssertIdentifierAt(result, 1000, controlNumber + "_1000");
		}

		private static void AssertIdentifierAt(IEnumerable<string> result, int index, string expectedIdentifier)
		{
			result.ElementAt(index).Should().Be(expectedIdentifier,
				"Image identifiers should have a number with leading zeros");
		}
	}
}
