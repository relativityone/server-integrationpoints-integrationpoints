using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
	[TestFixture]
	public class MassUpdateHelperTests
	{
		private MassUpdateHelper _sut;

		private Mock<IConfig> _configMock;
		private Mock<IAPILog> _loggerMock;
		private Mock<IRepositoryWithMassUpdate> _massUpdateRepositoryMock;
		private Mock<IScratchTableRepository> _scratchTableRepositoryMock;

		private const int _DEFAULT_BATCH_SIZE = 100;
		private const int _DEFAULT_NUMBER_OF_DOCUMENTS = 1000;

		[SetUp]
		public void SetUp()
		{
			_configMock = new Mock<IConfig>();
			SetupBatchSize(_DEFAULT_BATCH_SIZE);

			_loggerMock = new Mock<IAPILog>
			{
				DefaultValue = DefaultValue.Mock
			};

			_massUpdateRepositoryMock = new Mock<IRepositoryWithMassUpdate>();
			SetupMassUpdateResult(true);

			_scratchTableRepositoryMock = new Mock<IScratchTableRepository>();
			SetupNumberOfArtifacts(_DEFAULT_NUMBER_OF_DOCUMENTS);

			_sut = new MassUpdateHelper(
				_configMock.Object,
				_loggerMock.Object);
		}

		[Test]
		public void Constructor_ShouldThrowArgumentNullExceptionWhenConfigIsNull()
		{
			// act
			Action constructor = () => new MassUpdateHelper(
				config: null,
				logger: _loggerMock.Object);

			// assert
			constructor.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void Constructor_ShouldNotThrowExceptionWhenLoggerIsNull()
		{
			// act
			Action constructor = () => new MassUpdateHelper(
				_configMock.Object,
				logger: null);

			// assert
			constructor.ShouldNotThrow<Exception>();
		}

		[Test]
		public void UpdateArtifactsFromScratchTableAsync_ShouldThrowArgumentNullExceptionWhenScratchTableIsNull()
		{
			// act
			Func<Task> updateArtifactsFromScratchTableAction = () => _sut.UpdateArtifactsAsync(
					(IScratchTableRepository)null,
					new FieldUpdateRequestDto[0],
					_massUpdateRepositoryMock.Object);

			// assert
			updateArtifactsFromScratchTableAction.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public async Task UpdateArtifactsFromScratchTableAsync_ShouldNotAttemptToReadDataWhenNoDocumentsToTag()
		{
			// arrange
			SetupNumberOfArtifacts(0);

			// act
			await _sut.UpdateArtifactsAsync(
					_scratchTableRepositoryMock.Object,
					new FieldUpdateRequestDto[0],
					_massUpdateRepositoryMock.Object)
				.ConfigureAwait(false);

			// assert
			_scratchTableRepositoryMock.Verify(
				x => x.ReadArtifactIDs(
					It.IsAny<int>(),
					It.IsAny<int>()),
				Times.Never);
		}

		[TestCase(0, 1, 0)]
		[TestCase(1, 3, 1)]
		[TestCase(3, 3, 1)]
		[TestCase(4, 3, 2)]
		[TestCase(6, 3, 2)]
		public async Task UpdateArtifactsFromScratchTableAsync_ShouldReadDataInBatches(
			int totalNumberOfDocuments,
			int batchSize,
			int expectedNumberOfBatches)
		{
			// arrange
			SetupBatchSize(batchSize);
			SetupNumberOfArtifacts(totalNumberOfDocuments);

			IEnumerable<int> expectedOffsets = Enumerable
				.Repeat(0, expectedNumberOfBatches)
				.Select(x => x * batchSize);

			// act
			await _sut.UpdateArtifactsAsync(
				_scratchTableRepositoryMock.Object,
				new FieldUpdateRequestDto[0],
				_massUpdateRepositoryMock.Object)
				.ConfigureAwait(false);

			// assert
			foreach (int expectedOffset in expectedOffsets)
			{
				_scratchTableRepositoryMock.Verify(x => x.ReadArtifactIDs(expectedOffset, batchSize));
			}
		}

		[TestCase(0)]
		[TestCase(-1)]
		[Timeout(100)]
		public void UpdateArtifactsFromScratchTableAsync_ShouldThrowExceptionWhenBatchSizeIsLessThanOne(int batchSize)
		{
			// arrange
			SetupBatchSize(batchSize);
			string expectedExceptionMessage = $"Batch size for mass update has to be bigger than 0, but found {batchSize}";

			// act
			Func<Task> updateDocumentsAction = () => _sut.UpdateArtifactsAsync(
					_scratchTableRepositoryMock.Object,
					new FieldUpdateRequestDto[0],
					_massUpdateRepositoryMock.Object);

			// assert
			updateDocumentsAction
				.ShouldThrow<IntegrationPointsException>()
				.WithMessage(expectedExceptionMessage);
		}

		[TestCase(3, 3)]
		[TestCase(3, 0)]
		[TestCase(5, 18)]
		public async Task UpdateArtifactsFromScratchTableAsync_ShouldUpdateDocumentsInBatches(int batchSize, int numberOfArtifacts)
		{
			// arrange
			SetupBatchSize(batchSize);
			SetupNumberOfArtifacts(numberOfArtifacts);
			List<List<int>> artifactIDsBatches = SetupAndGetScratchTableArtifactsBatches(
				numberOfArtifacts,
				batchSize);

			Guid fieldGuid = Guid.NewGuid();
			int singleObjectArtifactID = 2312;
			IFieldValueDto fieldValueDto = new MultiObjectReferenceDto(singleObjectArtifactID);
			var fieldUpdateRequestDto = new FieldUpdateRequestDto(fieldGuid, fieldValueDto);

			FieldUpdateRequestDto[] fieldsToUpdate = { fieldUpdateRequestDto };

			// act
			await _sut.UpdateArtifactsAsync(
					_scratchTableRepositoryMock.Object,
					fieldsToUpdate,
					_massUpdateRepositoryMock.Object)
				.ConfigureAwait(false);

			// assert
			foreach (List<int> documentsIDsBatch in artifactIDsBatches)
			{
				_massUpdateRepositoryMock.Verify(
					x => x.MassUpdateAsync(
						documentsIDsBatch,
						It.Is<IEnumerable<FieldUpdateRequestDto>>(fields => ValidateFieldUpdateRequestContainArtifactID(fields, fieldGuid, singleObjectArtifactID)))
				);
			}
		}

		[Test]
		public void UpdateArtifactsFromScratchTableAsync_ShouldThrowExceptionWhenMassUpdateFailed()
		{
			// arrange
			const int batchSize = 10;
			const int numberOfArtifacts = 30;
			const int batchWithMassUpdateFailure = 1;

			SetupBatchSize(batchSize);
			SetupNumberOfArtifacts(numberOfArtifacts);
			List<List<int>> artifactsIDsBatches = SetupAndGetScratchTableArtifactsBatches(
				numberOfArtifacts,
				batchSize);

			for (int batchNumber = 0; batchNumber < artifactsIDsBatches.Count; batchNumber++)
			{
				bool isBatchWithFailure = batchNumber == batchWithMassUpdateFailure;
				SetupMassUpdateResult(artifactsIDsBatches[batchNumber], isBatchWithFailure);
			}

			// act
			Func<Task> tagDocumentsAction = () => _sut.UpdateArtifactsAsync(
				_scratchTableRepositoryMock.Object,
				new FieldUpdateRequestDto[0],
				_massUpdateRepositoryMock.Object);

			// assert
			tagDocumentsAction.ShouldThrow<IntegrationPointsException>();
			for (int batchNumber = batchWithMassUpdateFailure + 1; batchNumber < artifactsIDsBatches.Count; batchNumber++)
			{
				_massUpdateRepositoryMock.Verify(x =>
					x.MassUpdateAsync(
						artifactsIDsBatches[batchNumber],
						It.IsAny<IEnumerable<FieldUpdateRequestDto>>()
						),
					Times.Never);
			}
		}

		[Test]
		public void UpdateArtifactsFromScratchTableAsync_ShouldWorkWithNullLogger()
		{
			// arrange
			var sut = new MassUpdateHelper(
				_configMock.Object,
				logger: null);

			// act
			Func<Task> updateAction = () => sut.UpdateArtifactsAsync(
				_scratchTableRepositoryMock.Object,
				new FieldUpdateRequestDto[0],
				_massUpdateRepositoryMock.Object);

			// assert
			updateAction.ShouldNotThrow<Exception>();
		}

		private void SetupNumberOfArtifacts(int totalNumberOfArtifacts)
		{
			_scratchTableRepositoryMock
				.Setup(x => x.GetCount())
				.Returns(totalNumberOfArtifacts);
		}

		private void SetupBatchSize(int batchSize)
		{
			_configMock
				.Setup(x => x.MassUpdateBatchSize)
				.Returns(batchSize);
		}

		private List<List<int>> SetupAndGetScratchTableArtifactsBatches(int numberOfArtifacts, int batchSize)
		{
			var artifactIDsBatches = new List<List<int>>();
			for (int offset = 0; offset < numberOfArtifacts; offset += batchSize)
			{
				List<int> artifactsIDsBatch = Enumerable.Range(0, batchSize).ToList();
				artifactIDsBatches.Add(artifactsIDsBatch);

				_scratchTableRepositoryMock
					.Setup(x => x.ReadArtifactIDs(0, batchSize))
					.Returns(artifactsIDsBatch);
			}

			return artifactIDsBatches;
		}

		private void SetupMassUpdateResult(bool massUpdateResult)
		{
			_massUpdateRepositoryMock
				.Setup(x => x.MassUpdateAsync(
					It.IsAny<IEnumerable<int>>(),
					It.IsAny<IEnumerable<FieldUpdateRequestDto>>())
				).ReturnsAsync(massUpdateResult);
		}

		private void SetupMassUpdateResult(IEnumerable<int> objectIDs, bool massUpdateResult)
		{
			_massUpdateRepositoryMock
				.Setup(x => x.MassUpdateAsync(
					objectIDs,
					It.IsAny<IEnumerable<FieldUpdateRequestDto>>())
				).ReturnsAsync(massUpdateResult);
		}

		private static bool ValidateFieldUpdateRequestContainArtifactID(
			IEnumerable<FieldUpdateRequestDto> requests,
			Guid fieldGuid,
			int expectedArtifactID)
		{
			FieldUpdateRequestDto fieldUpdateRequest = requests
				.Single(x => x.FieldIdentifier == fieldGuid);
			var multiObjectReference = fieldUpdateRequest.NewValue as MultiObjectReferenceDto;
			return multiObjectReference?.ObjectReferences.SequenceEqual(new[] { expectedArtifactID }) ?? false;
		}
	}
}
