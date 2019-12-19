using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings.FieldClassifiers;
using kCura.Relativity.Client;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Field = Relativity.Services.Objects.DataContracts.Field;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API.FieldMappings
{
	[TestFixture]
	public class FieldsClassifierRunnerTests
	{
		private FieldsClassifierRunner _sut;
		private Mock<IObjectManager> _objectManagerMock;

		[SetUp]
		public void SetUp()
		{
			_objectManagerMock = new Mock<IObjectManager>();
			Mock<IServicesMgr> servicesMgrFake = new Mock<IServicesMgr>();
			servicesMgrFake.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>())).Returns(_objectManagerMock.Object);
			_sut = new FieldsClassifierRunner(servicesMgrFake.Object);
		}

		[Test]
		public async Task GetFilteredFieldsAsync_ShouldFilterOutFieldsNotVisibleToUser()
		{
			// Arrange
			const string autoMappedFieldName = "Auto mapped field";
			const string autoMappedFieldType = "Fixed length text";

			const string excludedFromAutoMapFieldName = "Excluded from auto-map field";
			const string notVisibleToUserFieldName = "Field not visible to user";

			SetupWorkspaceFields(new List<RelativityObject>()
			{
				CreateField(autoMappedFieldName, type: autoMappedFieldType, isIdentifier: true),
				CreateField(excludedFromAutoMapFieldName),
				CreateField(notVisibleToUserFieldName)
			});

			var classificationResult = new List<FieldClassificationResult>()
			{
				new FieldClassificationResult()
				{
					Name = autoMappedFieldName,
					ClassificationLevel = ClassificationLevel.AutoMap
				},
				new FieldClassificationResult()
				{
					Name = excludedFromAutoMapFieldName,
					ClassificationLevel = ClassificationLevel.ShowToUser
				},
				new FieldClassificationResult()
				{
					Name = notVisibleToUserFieldName,
					ClassificationLevel = ClassificationLevel.HideFromUser
				}
			};

			var classifierFake = new Mock<IFieldsClassifier>();
			classifierFake
				.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<RelativityObject>>(), It.IsAny<int>()))
				.ReturnsAsync(classificationResult);
			var fieldsClassifiers = new List<Mock<IFieldsClassifier>>()
			{
				classifierFake
			};

			// Act
			IList<FieldClassificationResult> filteredFields = await _sut.GetFilteredFieldsAsync(It.IsAny<int>(), fieldsClassifiers.Select(x => x.Object).ToList()).ConfigureAwait(false);

			// Assert
			filteredFields.Exists(x => x.Name == autoMappedFieldName && x.Type == autoMappedFieldType && x.IsIdentifier).Should().BeTrue();
			filteredFields.Exists(x => x.Name == excludedFromAutoMapFieldName).Should().BeTrue();
			filteredFields.Exists(x => x.Name == notVisibleToUserFieldName).Should().BeFalse();
		}

		[Test]
		public async Task GetFilteredFieldsAsync_ShouldReturnAllFields_WhenNoClassifiers()
		{
			// Arrange
			const int count = 3;
			SetupWorkspaceFields(Enumerable.Range(1, count).Select(x => CreateField(x.ToString())).ToList());

			// Act
			IList<FieldClassificationResult> filteredFields = await _sut.GetFilteredFieldsAsync(It.IsAny<int>(), new List<IFieldsClassifier>()).ConfigureAwait(false);

			// Assert
			filteredFields.Count.Should().Be(count);
			filteredFields.All(x => x.ClassificationLevel == ClassificationLevel.AutoMap).Should().BeTrue();
		}

		[Test]
		public async Task GetFilteredFieldsAsync_ShouldUseBatching_WhenQueryingWithObjectManager()
		{
			// Arrange
			MockSequence mockSequence = new MockSequence();
			QueryResult firstQueryResult = CreateQueryResult(Enumerable.Range(1, 2).Select(x => CreateField(x.ToString())).ToList());
			QueryResult secondQueryResult = CreateQueryResult(Enumerable.Range(3, 2).Select(x => CreateField(x.ToString())).ToList());
			QueryResult thirdQueryResult = CreateQueryResult(new List<RelativityObject>());

			_objectManagerMock
				.InSequence(mockSequence)
				.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 50))
				.ReturnsAsync(firstQueryResult)
				.Verifiable();

			_objectManagerMock
				.InSequence(mockSequence)
				.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 3, 50))
				.ReturnsAsync(secondQueryResult)
				.Verifiable();

			_objectManagerMock
				.InSequence(mockSequence)
				.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 5, 50))
				.ReturnsAsync(thirdQueryResult)
				.Verifiable();

			// Act
			await _sut.GetFilteredFieldsAsync(It.IsAny<int>(), new List<IFieldsClassifier>()).ConfigureAwait(false);

			// Assert
			_objectManagerMock.Verify();
		}

		[Test]
		public void GetFilteredFieldsAsync_ShouldRetryOnce()
		{
			// Arrange
			SetupWorkspaceFields(Enumerable.Range(1, 2).Select(x => CreateField(x.ToString())).ToList());

			Mock<IFieldsClassifier> classifierMock = new Mock<IFieldsClassifier>();
			classifierMock.Setup(x => x.ClassifyAsync(It.IsAny<ICollection<RelativityObject>>(), It.IsAny<int>())).Throws<InvalidOperationException>();

			// Act
			Func<Task> action = () => _sut.GetFilteredFieldsAsync(0, new List<IFieldsClassifier>()
			{
				classifierMock.Object
			});

			// Assert
			action.ShouldThrow<InvalidOperationException>();
			classifierMock.Verify(x => x.ClassifyAsync(It.IsAny<ICollection<RelativityObject>>(), It.IsAny<int>()), Times.Exactly(2));
		}

		[Test]
		public async Task GetFilteredFieldsAsync_ShouldBuildProperQueryForObjectManager()
		{
			// Arrange
			SetupWorkspaceFields(Enumerable.Range(1, 2).Select(x => CreateField(x.ToString())).ToList());

			// Act
			await _sut.GetFilteredFieldsAsync(It.IsAny<int>(), new List<IFieldsClassifier>()).ConfigureAwait(false);

			// Assert
			_objectManagerMock.Verify(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(req => ValidateQueryRequest(req)), It.IsAny<int>(), It.IsAny<int>()));
		}

		private QueryResult CreateQueryResult(List<RelativityObject> fields)
		{
			QueryResult queryResult = new QueryResult()
			{
				Objects = fields,
				ResultCount = fields.Count
			};
			return queryResult;
		}

		private void SetupWorkspaceFields(List<RelativityObject> fields)
		{
			_objectManagerMock
				.SetupSequence(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(CreateQueryResult(fields))
				.ReturnsAsync(CreateQueryResult(new List<RelativityObject>()));
		}

		private bool ValidateQueryRequest(QueryRequest request)
		{
			return request.ObjectType.ArtifactTypeID == (int)ArtifactType.Field &&
				request.Condition == "'FieldArtifactTypeID' == 10" &&
				request.Fields.Single().Name == "*" &&
				request.IncludeNameInQueryResult == true;
		}

		private RelativityObject CreateField(string name, int artifactID = 0, string type = "", bool isIdentifier = false)
		{
			return new RelativityObject()
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
		}
	}
}