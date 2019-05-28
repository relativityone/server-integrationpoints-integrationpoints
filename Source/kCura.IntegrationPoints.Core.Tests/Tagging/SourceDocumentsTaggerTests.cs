using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
	[TestFixture]
	public class SourceDocumentsTaggerTests
	{
		private SourceDocumentsTagger _sut;

		private Mock<IConfig> _configMock;
		private Mock<IAPILog> _loggerMock;
		private Mock<IDocumentRepository> _documentRepositoryMock;
		private Mock<IScratchTableRepository> _scratchTableRepositoryMock;

		private const int _DEFAULT_BATCH_SIZE = 100;
		private const int _DEFAULT_NUMBER_OF_DOCUMENTS = 1000;
		private const int _DEFAULT_DESTINATION_WORKSPACE_ID = 43284;
		private const int _DEFAULT_JOB_HISTORY_ID = 74322;

		[SetUp]
		public void SetUp()
		{
			_configMock = new Mock<IConfig>();
			SetupBatchSize(_DEFAULT_BATCH_SIZE);

			_loggerMock = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};

			_documentRepositoryMock = new Mock<IDocumentRepository>();
			SetupMassUpdateResult(true);

			_scratchTableRepositoryMock = new Mock<IScratchTableRepository>();
			SetupNumberOfDocuments(_DEFAULT_NUMBER_OF_DOCUMENTS);

			_sut = new SourceDocumentsTagger(
				_documentRepositoryMock.Object,
				_configMock.Object,
				_loggerMock.Object);
		}

		[Test]
		public void ConstructorShouldThrowArgumentNullExceptionWhenDocumentRepositoryIsNull()
		{
			// act
			Action constructor = () => new SourceDocumentsTagger(
				null,
				_configMock.Object,
				_loggerMock.Object);

			// assert
			constructor.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void ConstructorShouldThrowArgumentNullExceptionWhenConfigIsNull()
		{
			// act
			Action constructor = () => new SourceDocumentsTagger(
				_documentRepositoryMock.Object,
				null,
				_loggerMock.Object);

			// assert
			constructor.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void ConstructorShouldNotThrowExceptionWhenLoggerIsNull()
		{
			// act
			Action constructor = () => new SourceDocumentsTagger(
				_documentRepositoryMock.Object,
				_configMock.Object,
				null);

			// assert
			constructor.ShouldNotThrow<Exception>();
		}

		[Test]
		public void ShouldThrowArgumentNullExceptionWhenScratchTableIsNull()
		{
			// act
			Func<Task> tagDocumentsUsingNullScratchTableAction = () => _sut
				.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
					null,
					_DEFAULT_DESTINATION_WORKSPACE_ID,
					_DEFAULT_JOB_HISTORY_ID);

			// assert
			tagDocumentsUsingNullScratchTableAction.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public async Task ShouldNotAttemptToReadDataWhenNoDocumentsToTag()
		{
			// arrange
			SetupNumberOfDocuments(0);

			// act
			await _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
					_scratchTableRepositoryMock.Object,
					_DEFAULT_DESTINATION_WORKSPACE_ID,
					_DEFAULT_JOB_HISTORY_ID)
				.ConfigureAwait(false);

			// assert
			_scratchTableRepositoryMock.Verify(
				x => x.ReadDocumentIDs(
					It.IsAny<int>(),
					It.IsAny<int>()),
				Times.Never);
		}

		[TestCase(0, 1, 0)]
		[TestCase(1, 3, 1)]
		[TestCase(3, 3, 1)]
		[TestCase(4, 3, 2)]
		[TestCase(6, 3, 2)]
		public async Task ShouldReadDataInBatches(
			int totalNumberOfDocuments,
			int batchSize,
			int expectedNumberOfBatches)
		{
			// arrange
			SetupBatchSize(batchSize);
			SetupNumberOfDocuments(totalNumberOfDocuments);

			IEnumerable<int> expectedOffsets = Enumerable
				.Repeat(0, expectedNumberOfBatches)
				.Select(x => x * batchSize);

			// act
			await _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
				_scratchTableRepositoryMock.Object,
				_DEFAULT_DESTINATION_WORKSPACE_ID,
				_DEFAULT_JOB_HISTORY_ID)
				.ConfigureAwait(false);

			// assert
			foreach (int expectedOffset in expectedOffsets)
			{
				_scratchTableRepositoryMock.Verify(x => x.ReadDocumentIDs(expectedOffset, batchSize));
			}
		}

		[TestCase(0)]
		[TestCase(-1)]
		[Timeout(100)]
		public void ShouldThrowExceptionWhenBatchSizeIsLessThanOne(int batchSize)
		{
			// arrange
			SetupBatchSize(batchSize);
			string expectedExceptionMessage = $"Batch size for source documents tagging has to be bigger than 0, but found {batchSize}";

			// act
			Func<Task> tagDocumentsAction = () => _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
					_scratchTableRepositoryMock.Object,
					_DEFAULT_DESTINATION_WORKSPACE_ID,
					_DEFAULT_JOB_HISTORY_ID);

			// assert
			tagDocumentsAction
				.ShouldThrow<IntegrationPointsException>()
				.WithMessage(expectedExceptionMessage);
		}

		[TestCase(3, 3)]
		[TestCase(3, 0)]
		[TestCase(5, 18)]
		public async Task ShouldUpdateDocumentsInBatches(int batchSize, int numberOfDocuments)
		{
			// arrange
			SetupBatchSize(batchSize);
			SetupNumberOfDocuments(numberOfDocuments);

			var documentsIDsBatches = new List<List<int>>();
			for (int offset = 0; offset < numberOfDocuments; offset += batchSize)
			{
				List<int> documentsIDsBatch = Enumerable.Range(0, batchSize).ToList();
				documentsIDsBatches.Add(documentsIDsBatch);

				_scratchTableRepositoryMock
					.Setup(x => x.ReadDocumentIDs(0, batchSize))
					.Returns(documentsIDsBatch);
			}

			// act
			await _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
				_scratchTableRepositoryMock.Object,
				_DEFAULT_DESTINATION_WORKSPACE_ID,
				_DEFAULT_JOB_HISTORY_ID)
				.ConfigureAwait(false);

			// assert
			foreach (List<int> documentsIDsBatch in documentsIDsBatches)
			{
				_documentRepositoryMock.Verify(
					x => x.MassUpdateDocumentsAsync(
						documentsIDsBatch,
						It.Is<IEnumerable<FieldUpdateRequestDto>>(fields => ValidateTagsFieldsRequests(fields)))
				);
			}
		}

		[Test]
		public void ShouldThrowExceptionWhenMassUpdateFailed()
		{
			// arrange
			const int batchSize = 10;
			const int numberOfDocuments = 30;
			const int batchWithMassUpdateFailure = 1;

			SetupBatchSize(batchSize);
			SetupNumberOfDocuments(numberOfDocuments);

			var documentsIDsBatches = new List<List<int>>();
			for (int offset = 0; offset < numberOfDocuments; offset += batchSize)
			{
				List<int> documentsIDsBatch = Enumerable.Range(0, batchSize).ToList();
				documentsIDsBatches.Add(documentsIDsBatch);

				_scratchTableRepositoryMock
					.Setup(x => x.ReadDocumentIDs(0, batchSize))
					.Returns(documentsIDsBatch);
			}

			for (int batchNumber = 0; batchNumber < documentsIDsBatches.Count; batchNumber++)
			{
				bool isBatchWithFailure = batchNumber == batchWithMassUpdateFailure;
				SetupMassUpdateResult(documentsIDsBatches[batchNumber], isBatchWithFailure);
			}

			// act
			Func<Task> tagDocumentsAction = () => _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
				_scratchTableRepositoryMock.Object,
				_DEFAULT_DESTINATION_WORKSPACE_ID,
				_DEFAULT_JOB_HISTORY_ID);

			// assert
			tagDocumentsAction.ShouldThrow<IntegrationPointsException>();
			for (int batchNumber = batchWithMassUpdateFailure + 1; batchNumber < documentsIDsBatches.Count; batchNumber++)
			{
				_documentRepositoryMock.Verify(x =>
					x.MassUpdateDocumentsAsync(
						documentsIDsBatches[batchNumber],
						It.IsAny<IEnumerable<FieldUpdateRequestDto>>()
						),
					Times.Never);
			}
		}

		[Test]
		public void ShouldWorkWithNullLogger()
		{
			// arrange
			var sut = new SourceDocumentsTagger(
				_documentRepositoryMock.Object,
				_configMock.Object,
				logger: null);

			// act
			Func<Task> tagDocumentsAction = () => sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
				_scratchTableRepositoryMock.Object,
				_DEFAULT_DESTINATION_WORKSPACE_ID,
				_DEFAULT_JOB_HISTORY_ID);

			// assert
			tagDocumentsAction.ShouldNotThrow<Exception>();
		}

		private void SetupNumberOfDocuments(int totalNumberOfDocuments)
		{
			_scratchTableRepositoryMock
				.Setup(x => x.GetCount())
				.Returns(totalNumberOfDocuments);
		}

		private void SetupBatchSize(int batchSize)
		{
			_configMock
				.Setup(x => x.SourceWorkspaceTaggerBatchSize)
				.Returns(batchSize);
		}

		private void SetupMassUpdateResult(bool massUpdateResult)
		{
			_documentRepositoryMock
				.Setup(x => x.MassUpdateDocumentsAsync(
					It.IsAny<IEnumerable<int>>(),
					It.IsAny<IEnumerable<FieldUpdateRequestDto>>())
				).Returns(Task.FromResult(massUpdateResult));
		}

		private void SetupMassUpdateResult(IEnumerable<int> objectIDs, bool massUpdateResult)
		{
			_documentRepositoryMock
				.Setup(x => x.MassUpdateDocumentsAsync(
					objectIDs,
					It.IsAny<IEnumerable<FieldUpdateRequestDto>>())
				).Returns(Task.FromResult(massUpdateResult));
		}

		private bool ValidateTagsFieldsRequests(IEnumerable<FieldUpdateRequestDto> requests)
		{
			bool isValid = true;
			isValid &= ValidateFieldUpdateRequestContainArtifactID(
				requests,
				Guid.Parse(DocumentFieldGuids.RelativityDestinationCase),
				_DEFAULT_DESTINATION_WORKSPACE_ID);
			isValid &= ValidateFieldUpdateRequestContainArtifactID(
				requests,
				Guid.Parse(DocumentFieldGuids.JobHistory),
				_DEFAULT_JOB_HISTORY_ID);
			return isValid;
		}

		private static bool ValidateFieldUpdateRequestContainArtifactID(
			IEnumerable<FieldUpdateRequestDto> requests,
			Guid fieldGuid,
			int expectedArtifactID)
		{
			FieldUpdateRequestDto fieldUpdateRequest = requests.Single(x =>
				x.FieldIdentifier == fieldGuid);
			var multiObjectReference = fieldUpdateRequest.NewValue as MultiObjectReferenceDto;
			return multiObjectReference?.ObjectReferences.SequenceEqual(new[] { expectedArtifactID }) ?? false;
		}
	}
}
