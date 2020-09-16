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

			var DocumentToImageFiles = new Dictionary<int, IEnumerable<ImageFile>>()
			{
				{ documentId, new[] { new ImageFile(documentId, "Location1", "Name1", 0) } }
			};

			var notExistingDocument = new RelativityObjectSlim { ArtifactID = nonExsitingDocumentId };

			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);

			// Act
			var result = _sut.BuildRowValues(It.IsAny<FieldInfoDto>(), notExistingDocument);

			// Assert
			result.Should().BeEmpty();
		}

		[Test]
		public void BuildRowValues_ShouldReturnEmpty_WhenDocumentHasNotAnyImages()
		{
			// Arrange
			const int documentId = 1;
			const int documentWithouImagesId = 2;

			var DocumentToImageFiles = new Dictionary<int, IEnumerable<ImageFile>>()
			{
				{ documentId, new[] { new ImageFile(documentId, "Location1", "Name1", 0) } },
				{ documentWithouImagesId, new ImageFile[] { } }
			};

			var documentWithoutImages = new RelativityObjectSlim { ArtifactID = documentWithouImagesId };

			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);

			// Act
			var result = _sut.BuildRowValues(It.IsAny<FieldInfoDto>(), documentWithoutImages);

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
			var DocumentToImageFiles = new Dictionary<int, IEnumerable<ImageFile>>()
			{
				{ 1, new[] { new ImageFile(1, "Location1", "Name1", 0) } },
				{ 2, new[] { new ImageFile(2, "Location2a", "Name2a", 0), new ImageFile(2, "Location2b", "Name2b", 0), new ImageFile(2, "Location2c", "Name2c", 0) } },
				{ 3, new[] { new ImageFile(3, "Location3", "Name3", 0) } }
			};

			var document = new RelativityObjectSlim { ArtifactID = 2 };

			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);

			// Act
			var result = _sut.BuildRowValues(specialField, document);

			// Assert
			result.Should().BeEquivalentTo(expectedValues);
		}

		[Test]
		public void BuildRowValues_ShouldThrow_WhenSpecialFieldTypeIsNotAllowed()
		{
			// Arrange
			const int documentId = 1;

			var DocumentToImageFiles = new Dictionary<int, IEnumerable<ImageFile>>()
			{
				{ documentId, new[] { new ImageFile(documentId, "Location1", "Name1", 0) } }
			};

			var field = FieldInfoDto.NativeFileLocationField();

			var document = new RelativityObjectSlim { ArtifactID = documentId };

			_sut = new ImageInfoRowValuesBuilder(DocumentToImageFiles);

			// Act
			Func<object> action = () => _sut.BuildRowValues(field, document);

			// Assert
			action.Should().Throw<ArgumentException>();
		}
	}
}
