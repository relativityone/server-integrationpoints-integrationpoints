﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal class ImageInfoRowValuesBuilderTests
    {
        private ImageInfoRowValuesBuilder _sut;

        private Mock<IAntiMalwareHandler> _antiMalwareHandlerFake;

        private static IEnumerable<TestCaseData> SpecialFieldExpectedReturnValuesData()
            => new[]
                {
                    new TestCaseData(FieldInfoDto.ImageFileNameField(), new object[] { "Name2a", "Name2b", "Name2c" }),
                    new TestCaseData(FieldInfoDto.ImageFileLocationField(), new object[] { "Location2a", "Location2b", "Location2c" })
                };

        [SetUp]
        public void SetUp()
        {
            _antiMalwareHandlerFake = new Mock<IAntiMalwareHandler>();
        }

        [Test]
        public void BuildRowValues_ShouldReturnEmpty_WhenDocumentDoesNotExistInImageFiles()
        {
            // Arrange
            const int documentId = 1;
            const int nonExsitingDocumentId = 2;

            var documentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { documentId, new[] { new ImageFile(documentId, "1", "Location1", "Name1", 0) } }
            };

            var notExistingDocument = new RelativityObjectSlim { ArtifactID = nonExsitingDocumentId };

            _sut = PrepareSut(documentToImageFiles);

            // Act
            var result = _sut.BuildRowsValues(It.IsAny<FieldInfoDto>(), notExistingDocument, _ => string.Empty);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void BuildRowValues_ShouldReturnEmpty_WhenDocumentHasNotAnyImages()
        {
            // Arrange
            const int documentId = 1;
            const int documentWithouImagesId = 2;

            var documentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { documentId, new[] { new ImageFile(documentId, "1", "Location1", "Name1", 0) } },
                { documentWithouImagesId, new ImageFile[] { } }
            };

            var documentWithoutImages = new RelativityObjectSlim { ArtifactID = documentWithouImagesId };

            _sut = PrepareSut(documentToImageFiles);

            // Act
            var result = _sut.BuildRowsValues(It.IsAny<FieldInfoDto>(), documentWithoutImages, _ => string.Empty);

            // Assert
            result.Should().BeEmpty();
        }

        [TestCaseSource(nameof(SpecialFieldExpectedReturnValuesData))]
        public void BuildRowValues_ShouldValues_WhenSpecialFieldTypeHasBeenProvided(FieldInfoDto specialField, IEnumerable<object> expectedValues)
        {
            // Arrange
            var documentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { 1, new[] { new ImageFile(1, "1", "Location1", "Name1", 0) } },
                { 2, new[] { new ImageFile(2, "2a", "Location2a", "Name2a", 0), new ImageFile(2, "2b", "Location2b", "Name2b", 0), new ImageFile(2, "2c", "Location2c", "Name2c", 0) } },
                { 3, new[] { new ImageFile(3, "3", "Location3", "Name3", 0) } }
            };

            var document = new RelativityObjectSlim { ArtifactID = 2 };

            _sut = PrepareSut(documentToImageFiles);

            // Act
            var result = _sut.BuildRowsValues(specialField, document, _ => string.Empty);

            // Assert
            result.Should().BeEquivalentTo(expectedValues);
        }

        [Test]
        public void BuildRowValues_ShouldThrow_WhenSpecialFieldTypeIsNotAllowed()
        {
            // Arrange
            const int documentId = 1;

            var documentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { documentId, new[] { new ImageFile(documentId, "1", "Location1", "Name1", 0) } }
            };

            var field = FieldInfoDto.NativeFileLocationField();

            var document = new RelativityObjectSlim { ArtifactID = documentId };

            _sut = PrepareSut(documentToImageFiles);

            // Act
            Func<object> action = () => _sut.BuildRowsValues(field, document, _ => string.Empty);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void BuildRowValues_Should_GenerateCorrectIdentifier()
        {
            // Arrange
            const int documentId = 1;

            var documentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { documentId, Enumerable.Range(1, 1001).Select(x => new ImageFile(documentId, $"identifier_{x}", $"location_{x}", $"filename_{x}", 5)).ToArray() }
            };

            var field = FieldInfoDto.ImageIdentifierField();

            var document = new RelativityObjectSlim { ArtifactID = documentId };

            _sut = PrepareSut(documentToImageFiles);

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

        [Test]
        public void BuildRowValues_ShouldThrowItemLevelError_WhenMalwareWasDetected()
        {
            // Arrange
            const int documentId = 1;

            ImageFile malwareImageFile = new ImageFile(documentId, "1", "Location1", "Name1", 0);

            var documentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                { documentId, new[] { malwareImageFile } }
            };

            var field = FieldInfoDto.ImageFileNameField();

            var document = new RelativityObjectSlim { ArtifactID = documentId };

            _sut = PrepareSut(documentToImageFiles);

            _antiMalwareHandlerFake.Setup(x => x.ContainsMalwareAsync(malwareImageFile)).ReturnsAsync(true);

            // Act
            Func<object> action = () => _sut.BuildRowsValues(field, document, _ => string.Empty);

            // Assert
            action.Should().Throw<SyncItemLevelErrorException>();
        }

        [Test]
        public void BuildRowValues_ShouldCheckAllImagesForMalware_WhenMalwareWasDetectedForFirstImage()
        {
            // Arrange
            const int documentId = 1;

            ImageFile malwareImageFile1 = new ImageFile(documentId, "1", "Location1", "Name1", 0);
            ImageFile malwareImageFile2 = new ImageFile(documentId, "2", "Location2", "Name2", 0);
            ImageFile malwareImageFile3 = new ImageFile(documentId, "3", "Location3", "Name3", 0);

            var documentToImageFiles = new Dictionary<int, ImageFile[]>()
            {
                {
                    documentId,
                    new[]
                    {
                        malwareImageFile1,
                        malwareImageFile2,
                        malwareImageFile3
                    }
                }
            };

            var field = FieldInfoDto.ImageFileNameField();

            var document = new RelativityObjectSlim { ArtifactID = documentId };

            _sut = PrepareSut(documentToImageFiles);

            _antiMalwareHandlerFake.Setup(x => x.ContainsMalwareAsync(malwareImageFile1)).ReturnsAsync(true);
            _antiMalwareHandlerFake.Setup(x => x.ContainsMalwareAsync(malwareImageFile3)).ReturnsAsync(true);

            // Act
            Func<object> action = () => _sut.BuildRowsValues(field, document, _ => string.Empty);

            // Assert
            action.Should().Throw<SyncItemLevelErrorException>().WithMessage($"*{malwareImageFile1.Location}*{malwareImageFile3.Location}*");
        }

        private ImageInfoRowValuesBuilder PrepareSut(IDictionary<int, ImageFile[]> documentToImageFiles)
        {
            return new ImageInfoRowValuesBuilder(documentToImageFiles, _antiMalwareHandlerFake.Object);
        }

        private void AssertIdentifierAt(IEnumerable<string> result, int index, string expectedIdentifier)
        {
            result.ElementAt(index).Should().Be(expectedIdentifier, "Image identifiers should have a number with leading zeros");
        }
    }
}
