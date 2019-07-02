using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Tests.Repositories.Implementations.CommonTests;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class KeplerDocumentRepositoryTests
	{
		private KeplerDocumentRepository _sut;

		private Mock<IRelativityObjectManager> _objectManagerMock;

		private MassUpdateTests _massUpdateTests;

		[SetUp]
		public void SetUp()
		{
			_objectManagerMock = new Mock<IRelativityObjectManager>();
			_sut = new KeplerDocumentRepository(_objectManagerMock.Object);

			_massUpdateTests = new MassUpdateTests(_sut, _objectManagerMock);
		}

		[Test]
		public async Task RetrieveDocumentsArtifactIDsAsync_ShouldBuildProperRequest()
		{
			// arrange
			const string documentIdentifierField = "CONTROL NUMBER";
			int[] artifactsIDs = { 5, 9843, 3212 };
			string[] documentsIdentifiers = artifactsIDs.Select(x => x.ToString()).ToArray();
			List<RelativityObject> response = artifactsIDs
				.Select(artifactID => new RelativityObject { ArtifactID = artifactID })
				.ToList();

			_objectManagerMock
				.Setup(x => x.QueryAsync(
					It.IsAny<QueryRequest>(),
					It.IsAny<ExecutionIdentity>()))
				.Returns(Task.FromResult(response));

			Func<QueryRequest, bool> queryRequestValidator = queryRequest =>
			{
				bool isValid = true;
				isValid &= queryRequest.Condition == @"'CONTROL NUMBER' in ['5','9843','3212']";
				isValid &= queryRequest.Fields.Any(field => field.Name == "Artifact ID");
				isValid &= queryRequest.ObjectType.ArtifactTypeID ==
						   kCura.IntegrationPoint.Tests.Core.Constants.DOCUMENT_ARTIFACT_TYPE_ID;
				return isValid;
			};

			// act
			await _sut.RetrieveDocumentsAsync(documentIdentifierField, documentsIdentifiers).ConfigureAwait(false);

			// assert
			_objectManagerMock.Verify(
				x => x.QueryAsync(It.Is<QueryRequest>(query => queryRequestValidator(query)),
					ExecutionIdentity.CurrentUser));
		}

		[Test]
		public async Task RetrieveDocumentsArtifactIDsAsync_ShouldReturnArtifactIDs()
		{
			// arrange
			int[] artifactsIDs = { 5, 9843, 3212 };
			List<RelativityObject> response = artifactsIDs
				.Select(artifactID => new RelativityObject { ArtifactID = artifactID })
				.ToList();

			_objectManagerMock
				.Setup(x => x.QueryAsync(
					It.IsAny<QueryRequest>(),
					It.IsAny<ExecutionIdentity>()))
				.Returns(Task.FromResult(response));

			// act
			int[] result = await _sut
				.RetrieveDocumentsAsync(
					docIdentifierField: string.Empty,
					docIdentifierValues: new[] { string.Empty })
				.ConfigureAwait(false);

			// assert
			result.Should().BeEquivalentTo(artifactsIDs);
		}

		[Test]
		public void RetrieveDocumentsArtifactIDsAsync_ShouldRethrowObjectManagerException()
		{
			// arrange
			var exceptionToThrow = new Exception();
			_objectManagerMock
				.Setup(x => x.QueryAsync(
					It.IsAny<QueryRequest>(),
					It.IsAny<ExecutionIdentity>()))
				.Throws(exceptionToThrow);

			// act
			Func<Task> retrieveAction = () => _sut
				.RetrieveDocumentsAsync(
					docIdentifierField: string.Empty,
					docIdentifierValues: new[] { string.Empty });

			// assert
			retrieveAction.ShouldThrow<Exception>()
				.Which.Should().Be(exceptionToThrow);
		}

		[Test]
		public async Task RetrieveDocumentsAsync_ShouldBuildProperRequest()
		{
			// arrange
			int[] artifactsIDs = { 5, 9843, 3212 };
			var fieldIDs = new HashSet<int> { 597, 412 };
			List<RelativityObject> response = artifactsIDs
				.Select(
					artifactID => new RelativityObject
					{
						ArtifactID = artifactID,
						FieldValues = new List<FieldValuePair>()
					})
				.ToList();

			_objectManagerMock
				.Setup(x => x.QueryAsync(
					It.IsAny<QueryRequest>(),
					It.IsAny<ExecutionIdentity>()))
				.Returns(Task.FromResult(response));

			Func<QueryRequest, bool> queryRequestValidator = queryRequest =>
			{
				bool isValid = true;
				isValid &= queryRequest.Condition == @"'ArtifactID' in [5,9843,3212]";
				foreach (int fieldID in fieldIDs)
				{
					isValid &= queryRequest.Fields.Any(field => field.ArtifactID == fieldID);
				}
				isValid &= queryRequest.ObjectType.ArtifactTypeID ==
						   kCura.IntegrationPoint.Tests.Core.Constants.DOCUMENT_ARTIFACT_TYPE_ID;
				return isValid;
			};

			// act
			await _sut
				.RetrieveDocumentsAsync(artifactsIDs, fieldIDs)
				.ConfigureAwait(false);

			// assert
			_objectManagerMock.Verify(
				x => x.QueryAsync(It.Is<QueryRequest>(query => queryRequestValidator(query)),
					ExecutionIdentity.CurrentUser));
		}

		[Test]
		public async Task RetrieveDocumentsAsync_ShouldReturnArtifactDTOs()
		{
			// arrange
			const int firstFieldArtifactID = 597;
			const int secondFieldArtifactID = 412;
			int[] artifactsIDs = { 5, 9843, 3212 };
			var fieldIDs = new HashSet<int> { firstFieldArtifactID, secondFieldArtifactID };
			List<RelativityObject> response = artifactsIDs
				.Select(
					artifactID => new RelativityObject
					{
						ArtifactID = artifactID,
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field
								{
									ArtifactID = firstFieldArtifactID
								},
								Value = string.Empty
							},
							new FieldValuePair
							{
								Field = new Field
								{
									ArtifactID = secondFieldArtifactID
								},
								Value = string.Empty
							}
						}
					})
				.ToList();

			_objectManagerMock
				.Setup(x => x.QueryAsync(
					It.IsAny<QueryRequest>(),
					It.IsAny<ExecutionIdentity>()))
				.Returns(Task.FromResult(response));

			// act
			ArtifactDTO[] result = await _sut
				.RetrieveDocumentsAsync(artifactsIDs, fieldIDs)
				.ConfigureAwait(false);

			// assert
			result.Select(x => x.ArtifactId).Should()
				.BeEquivalentTo(
					artifactsIDs,
					"because all requested documents should be returned");
			foreach (ArtifactDTO artifactDto in result)
			{
				artifactDto.Fields.Select(x => x.ArtifactId).Should()
					.BeEquivalentTo(
						fieldIDs,
						"because for each documents each field should be present");
			}
		}

		[Test]
		public void RetrieveDocumentsAsync_ShouldRethrowObjectManagerException()
		{
			// arrange
			var exceptionToThrow = new Exception();
			_objectManagerMock
				.Setup(x => x.QueryAsync(
					It.IsAny<QueryRequest>(),
					It.IsAny<ExecutionIdentity>()))
				.Throws(exceptionToThrow);

			// act
			Func<Task> retrieveDocumentsAction = () => _sut
				.RetrieveDocumentsAsync(Enumerable.Empty<int>(), new HashSet<int>());

			// assert
			retrieveDocumentsAction.ShouldThrow<Exception>()
				.Which.Should().Be(exceptionToThrow);
		}

		[Test]
		public async Task RetrieveDocumentByIdentifierPrefixAsync_ShouldBuildProperRequest()
		{
			// arrange
			const string documentIdentifierField = "CONTROL NUMBER";
			const string identifierPrefix = "ZIPPER";
			var response = new List<Document>();

			_objectManagerMock
				.Setup(x => x.QueryAsync<Document>(
					It.IsAny<QueryRequest>(),
					It.IsAny<bool>(),
					It.IsAny<ExecutionIdentity>()))
				.Returns(Task.FromResult(response));

			Func<QueryRequest, bool> queryRequestValidator = queryRequest =>
			{
				bool isValid = true;
				isValid &= queryRequest.Condition == @"'CONTROL NUMBER' like 'ZIPPER%'";
				return isValid;
			};

			// act
			await _sut
				.RetrieveDocumentByIdentifierPrefixAsync(documentIdentifierField, identifierPrefix)
				.ConfigureAwait(false);

			// assert
			_objectManagerMock.Verify(
				x => x.QueryAsync<Document>(
					It.Is<QueryRequest>(query => queryRequestValidator(query)),
					true,
					ExecutionIdentity.CurrentUser));
		}

		[Test]
		public async Task RetrieveDocumentByIdentifierPrefixAsync_ShouldReturnArtifactIDs()
		{
			// arrange
			const string documentIdentifierField = "CONTROL NUMBER";
			const string identifierPrefix = "ZIPPER";
			var documentsIDS = new List<int> { 5324, 546596, 31232, 312412, 32132 };
			List<Document> response = documentsIDS.Select(id => new Document { ArtifactId = id }).ToList();

			_objectManagerMock
				.Setup(x => x.QueryAsync<Document>(
					It.IsAny<QueryRequest>(),
					It.IsAny<bool>(),
					It.IsAny<ExecutionIdentity>()))
				.Returns(Task.FromResult(response));

			// act
			int[] result = await _sut
				.RetrieveDocumentByIdentifierPrefixAsync(documentIdentifierField, identifierPrefix)
				.ConfigureAwait(false);

			// assert
			result.Should().BeEquivalentTo(documentsIDS);
		}

		[Test]
		public void RetrieveDocumentByIdentifierPrefixAsync_ShouldRethrowObjectManagerException()
		{
			// arrange
			var exceptionToThrow = new Exception();
			_objectManagerMock
				.Setup(x => x.QueryAsync<Document>(
					It.IsAny<QueryRequest>(),
					It.IsAny<bool>(),
					It.IsAny<ExecutionIdentity>()))
				.Throws(exceptionToThrow);

			// act
			Func<Task> retrieveDocumentsAction = () => _sut
				.RetrieveDocumentByIdentifierPrefixAsync(string.Empty, string.Empty);

			// assert
			retrieveDocumentsAction.ShouldThrow<Exception>()
				.Which.Should().Be(exceptionToThrow);
		}

		[Test]
		public Task MassUpdateAsync_ShouldBuildProperRequest()
		{
			return _massUpdateTests.ShouldBuildProperRequest();
		}

		[TestCase(true)]
		[TestCase(false)]
		public Task MassUpdateAsync_ShouldReturnCorrectResult(bool expectedResult)
		{
			return _massUpdateTests.ShouldReturnCorrectResult(expectedResult);
		}

		[Test]
		public void MassUpdateAsync_ShouldRethrowObjectManagerException()
		{
			_massUpdateTests.ShouldRethrowObjectManagerException();
		}
	}
}
