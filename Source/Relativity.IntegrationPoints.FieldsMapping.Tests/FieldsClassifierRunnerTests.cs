﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using Relativity.Services.Objects.DataContracts;
using Field = Relativity.Services.Objects.DataContracts.Field;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API.FieldMappings
{
	[TestFixture, Category("Unit")]
	public class FieldsClassifierRunnerTests
	{
		private Mock<IFieldsRepository> _fieldsRepositoryFake;

		[SetUp]
		public void SetUp()
		{
			_fieldsRepositoryFake = new Mock<IFieldsRepository>();
		}

		[Test]
		public async Task GetFilteredFieldsAsync_ShouldFilterOutFieldsNotVisibleToUser()
		{
			// Arrange
			const string autoMappedFieldName = "Auto mapped field";
			const string autoMappedFieldType = "Fixed length text";

			const string excludedFromAutoMapFieldName = "Excluded from auto-map field";
			const string notVisibleToUserFieldName = "Field not visible to user";

			var workspaceFields = new List<DocumentFieldInfo>()
			{
				CreateField(autoMappedFieldName, type: autoMappedFieldType, isIdentifier: true),
				CreateField(excludedFromAutoMapFieldName),
				CreateField(notVisibleToUserFieldName)
			};

			SetupWorkspaceFields(workspaceFields);

			var classificationResult = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(workspaceFields[0]) { ClassificationLevel = ClassificationLevel.AutoMap },
				new FieldClassificationResult(workspaceFields[1]) { ClassificationLevel = ClassificationLevel.ShowToUser },
				new FieldClassificationResult(workspaceFields[2]) { ClassificationLevel = ClassificationLevel.HideFromUser }
			};

			var classifierFake = new Mock<IFieldsClassifier>();
			classifierFake
				.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<DocumentFieldInfo>>(), It.IsAny<int>()))
				.ReturnsAsync(classificationResult);
			var fieldsClassifiers = new List<IFieldsClassifier>()
			{
				classifierFake.Object
			};

			FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);

			// Act
			IList<FieldClassificationResult> filteredFields = await sut.GetFilteredFieldsAsync(It.IsAny<int>()).ConfigureAwait(false);

			// Assert
			filteredFields.Any(x => x.Name == autoMappedFieldName && x.Type == autoMappedFieldType && x.IsIdentifier).Should().BeTrue();
			filteredFields.Any(x => x.Name == excludedFromAutoMapFieldName).Should().BeTrue();
			filteredFields.Any(x => x.Name == notVisibleToUserFieldName).Should().BeFalse();
		}

		[Test]
		public async Task GetFilteredFieldsAsync_ShouldReturnAllFields_WhenNoClassifiers()
		{
			// Arrange
			const int count = 3;
			SetupWorkspaceFields(Enumerable.Range(1, count).Select(x => CreateField(x.ToString())).ToList());

			FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object);

			// Act
			IList<FieldClassificationResult> filteredFields = await sut.GetFilteredFieldsAsync(It.IsAny<int>()).ConfigureAwait(false);

			// Assert
			filteredFields.Count.Should().Be(count);
			filteredFields.All(x => x.ClassificationLevel == ClassificationLevel.AutoMap).Should().BeTrue();
		}

		[Test]
		public void GetFilteredFieldsAsync_ShouldRetryOnce()
		{
			// Arrange
			SetupWorkspaceFields(Enumerable.Range(1, 2).Select(x => CreateField(x.ToString())).ToList());

			Mock<IFieldsClassifier> classifierMock = new Mock<IFieldsClassifier>();
			classifierMock.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<DocumentFieldInfo>>(), It.IsAny<int>())).Throws<InvalidOperationException>();

			var fieldsClassifiers = new List<IFieldsClassifier>()
			{
				classifierMock.Object
			};

			FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);

			// Act
			Func<Task> action = () => sut.GetFilteredFieldsAsync(0);

			// Assert
			action.ShouldThrow<InvalidOperationException>();
			classifierMock.Verify(x => x.ClassifyAsync(It.IsAny<ICollection<DocumentFieldInfo>>(), It.IsAny<int>()), Times.Exactly(2));
		}

		[Test]
		public async Task GetFilteredFieldsAsync_ShouldUpdateOnlyClassificationLevelAndReasonFromClassifier()
		{
			// Arrange
			const int count = 3;
			SetupWorkspaceFields(Enumerable.Range(1, count).Select(x => CreateField(x.ToString(), x, "Some type")).ToList());

			var classificationResult = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult(CreateField(name: "1", type: "Changed type", isIdentifier: true))
				{
					ClassificationLevel = ClassificationLevel.AutoMap,
					ClassificationReason = "Reason"
				},
				new FieldClassificationResult(CreateField(name: "2", type: "Changed type", isIdentifier: true))
				{
					ClassificationLevel = ClassificationLevel.ShowToUser,
					ClassificationReason = "Reason"
				},
				new FieldClassificationResult(CreateField(name: "3", type: "Changed type", isIdentifier: true))
				{
					ClassificationLevel = ClassificationLevel.ShowToUser,
					ClassificationReason = "Reason",
				}
			};

			var classifierFake = new Mock<IFieldsClassifier>();
			classifierFake
				.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<DocumentFieldInfo>>(), It.IsAny<int>()))
				.ReturnsAsync(classificationResult);

			var fieldsClassifiers = new List<IFieldsClassifier>()
			{
				classifierFake.Object
			};

			FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);


			// Act
			IList<FieldClassificationResult> filteredFields = await sut.GetFilteredFieldsAsync(It.IsAny<int>()).ConfigureAwait(false);

			// Assert
			filteredFields.Count.Should().Be(count);

			filteredFields.Select(x => x.IsIdentifier).Any(x => x).Should().BeFalse();
			filteredFields.Select(x => x.IsRequired).Any(x => x).Should().BeFalse();
			filteredFields.Select(x => x.Type).All(x => x == "Some type").Should().BeTrue();


			filteredFields[0].FieldIdentifier.Should().Be("1");
			filteredFields[1].FieldIdentifier.Should().Be("2");
			filteredFields[2].FieldIdentifier.Should().Be("3");

			filteredFields[0].ClassificationReason.Should().Be(null);
			filteredFields[1].ClassificationReason.Should().Be("Reason");
			filteredFields[2].ClassificationReason.Should().Be("Reason");
		}

		[Test]
		public async Task GetFilteredFieldsAsync_ShouldSortByName()
		{
			// Arrange
			List<DocumentFieldInfo> unsortedFields = new List<DocumentFieldInfo>()
			{
				CreateField("Field C"),
				CreateField("Field A"),
				CreateField("Field B")
			};

			List<DocumentFieldInfo> sortedFields = unsortedFields.OrderBy(x => x.Name).ToList();

			SetupWorkspaceFields(unsortedFields);

			Mock<IFieldsClassifier> classifier = new Mock<IFieldsClassifier>();
			IEnumerable<FieldClassificationResult> classificationResult = unsortedFields.Select(x => new FieldClassificationResult(x));

			classifier.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<DocumentFieldInfo>>(), It.IsAny<int>())).ReturnsAsync(classificationResult);

			var fieldsClassifiers = new List<IFieldsClassifier>()
			{
				classifier.Object
			};

			FieldsClassifierRunner sut = new FieldsClassifierRunner(_fieldsRepositoryFake.Object, fieldsClassifiers);

			// Act
			IList<FieldClassificationResult> filteredFields = await sut.GetFilteredFieldsAsync(0).ConfigureAwait(false);

			// Assert
			filteredFields.Select(x => x.Name).ShouldAllBeEquivalentTo(sortedFields.Select(x => x.Name));
		}

		private void SetupWorkspaceFields(IEnumerable<DocumentFieldInfo> fields)
		{
			_fieldsRepositoryFake
				.Setup(x => x.GetAllDocumentFieldsAsync(It.IsAny<int>()))
				.ReturnsAsync(fields);
		}

		private DocumentFieldInfo CreateField(string name, int artifactID = 0, string type = "", bool isIdentifier = false)
		{
			var fieldObject = new RelativityObject()
			{
				ArtifactID = artifactID,
				Name = name,
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Is Identifier"
						},
						Value = isIdentifier
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Field Type"
						},
						Value = type
					}
				}
			};

			return FieldConvert.ToDocumentFieldInfo(fieldObject);
		}
	}
}