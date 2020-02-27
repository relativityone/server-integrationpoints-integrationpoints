﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Relativity.ImportAPI;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Field = kCura.Relativity.ImportAPI.Data.Field;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests.FieldsClassifiers
{
	[TestFixture, Category("Unit")]
	public class NotSupportedByIAPIFieldsClassifierTests
	{
		private Mock<IImportAPI> _importApiFake;
		private NotSupportedByIAPIFieldsClassifier _sut;

		[SetUp]
		public void SetUp()
		{
			_importApiFake = new Mock<IImportAPI>();
			_sut = new NotSupportedByIAPIFieldsClassifier(_importApiFake.Object);
		}

		[Test]
		public async Task ClassifyAsync_ShouldProperlyClassifyFieldsNotSupportedByIAPI()
		{
			// Arrange
			List<DocumentFieldInfo> allFields = Enumerable.Range(1, 2).Select(x => new DocumentFieldInfo(
				fieldIdentifier: x.ToString(),
				name: $"Field {x}",
				type: "Fixed-Length Text(250)")).ToList();

			IEnumerable<Field> iapiFields = allFields.Where(x => x.FieldIdentifier == "1").Select(x => CreateIAPIField(int.Parse(x.FieldIdentifier), x.Name));
			_importApiFake.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), (int) ArtifactType.Document)).Returns(iapiFields);

			// Act
			List<FieldClassificationResult> fields = (await _sut.ClassifyAsync(allFields, 0).ConfigureAwait(false)).ToList();

			// Assert
			fields.Count().Should().Be(1);
			FieldClassificationResult fieldClassificationResult = fields.First();
			fieldClassificationResult.FieldIdentifier.Should().Be("2");
			fieldClassificationResult.ClassificationLevel.Should().Be(ClassificationLevel.HideFromUser);
			fieldClassificationResult.ClassificationReason.Should().Be("Field not supported by IAPI.");
		}

		private Field CreateIAPIField(int artifactID, string name)
		{
			Field field = new Field();
			Type fieldType = field.GetType();
			fieldType.GetProperty(nameof(Field.ArtifactID))?.SetValue(field, artifactID);
			fieldType.GetProperty(nameof(Field.Name))?.SetValue(field, name);
			return field;
		}

		[Test]
		public void ClassifyAsync_ShouldRethrowExceptionWhenGettingFieldsFromIapiFails()
		{
			// Arrange
			_importApiFake.Setup(x => x.GetWorkspaceFields(It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

			// Act
			Func<Task> action = () => _sut.ClassifyAsync(Mock.Of<ICollection<DocumentFieldInfo>>(), 0);

			// Assert
			action.Should().Throw<InvalidOperationException>();
		}
	}
}