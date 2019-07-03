﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Tagging
{
	[TestFixture]
	public class SourceDocumentsTaggerTests : TestBase
	{
		private Mock<IAPILog> _loggerMock;
		private Mock<IDocumentRepository> _documentRepositoryMock;
		private Mock<IMassUpdateHelper> _massUpdateHelperMock;
		private Mock<IScratchTableRepository> _documentsToTagRepositoryMock;

		private SourceDocumentsTagger _sut;
		private const int _DESTINATION_WORKSPACE_INSTANCE_ID = 1357475;
		private const int _JOB_HISTORY_INSTANCE_ID = 1357475;
		private const int _EXPECTED_NUMBER_OF_FIELD_UPDATE_REQUEST_DTO = 2;

		[SetUp]
		public override void SetUp()
		{
			_documentRepositoryMock = new Mock<IDocumentRepository>();
			_loggerMock = new Mock<IAPILog>() { DefaultValue = DefaultValue.Mock };
			_massUpdateHelperMock = new Mock<IMassUpdateHelper>();
			_documentsToTagRepositoryMock = new Mock<IScratchTableRepository>();

			_sut = new SourceDocumentsTagger(_documentRepositoryMock.Object, _loggerMock.Object, _massUpdateHelperMock.Object);
		}

		[Test]
		public void Constructor_ShouldThrowOnDocumentsRepositoryNull()
		{
			//act
			Action action = () => new SourceDocumentsTagger(
				documentRepository: null,
				logger: _loggerMock.Object,
				massUpdateHelper: _massUpdateHelperMock.Object);

			//assert
			action.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void Constructor_ShouldThrowOnLoggerNull()
		{
			//act
			Action action = () => new SourceDocumentsTagger(
				_documentRepositoryMock.Object,
				logger: null,
				massUpdateHelper: _massUpdateHelperMock.Object);

			//assert
			action.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void Constructor_ShouldThrowOnMassUpdateHelperNull()
		{
			//act
			Action action = () => new SourceDocumentsTagger(
				_documentRepositoryMock.Object,
				_loggerMock.Object,
				massUpdateHelper: null);

			//assert
			action.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void ShouldThrowOnMassUpdateHelperException()
		{
			//arrange
			var massUpdateException = new InvalidOperationException();
			_massUpdateHelperMock
				.Setup(
					x => x.UpdateArtifactsAsync(
						It.IsAny<IScratchTableRepository>(),
						It.IsAny<FieldUpdateRequestDto[]>(),
						It.IsAny<IDocumentRepository>()))
				.Throws(massUpdateException);
			//act
			Func<Task> action = () => _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
				_documentsToTagRepositoryMock.Object,
				_DESTINATION_WORKSPACE_INSTANCE_ID,
				_JOB_HISTORY_INSTANCE_ID);
			//assert
			action.ShouldThrow<InvalidOperationException>().Which.Should().Be(massUpdateException);
		}

		[Test]
		public async Task ShouldVerifyObjectsInRequest()
		{

			//act
			await _sut
				.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
					_documentsToTagRepositoryMock.Object,
					_DESTINATION_WORKSPACE_INSTANCE_ID,
					_JOB_HISTORY_INSTANCE_ID)
				.ConfigureAwait(false);

			//assert
			_massUpdateHelperMock
				.Verify(
					h => h.UpdateArtifactsAsync(
						_documentsToTagRepositoryMock.Object,
						It.Is<FieldUpdateRequestDto[]>(f => VerifyFields(f)),
						_documentRepositoryMock.Object),
					Times.Once);
		}

		[Test]
		public void ShouldThrowOnDocumentsToTagRepositoryNull()
		{
			//act
			Func<Task> action = () => _sut.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
				documentsToTagRepository: null,
				destinationWorkspaceInstanceID: _DESTINATION_WORKSPACE_INSTANCE_ID,
				jobHistoryInstanceID: _JOB_HISTORY_INSTANCE_ID);
			//assert
			action.ShouldThrow<ArgumentNullException>();
		}


		private bool VerifyFields(FieldUpdateRequestDto[] fieldUpdateRequestDtos)
		{
			if (fieldUpdateRequestDtos.Length != _EXPECTED_NUMBER_OF_FIELD_UPDATE_REQUEST_DTO)
			{
				return false;
			}

			bool result = true;
			result &= IsFieldUpdateRequestValid(
				fieldUpdateRequestDtos,
				DocumentFieldGuids.JobHistoryGuid,
				_JOB_HISTORY_INSTANCE_ID);
			result &= IsFieldUpdateRequestValid(
				fieldUpdateRequestDtos,
				DocumentFieldGuids.RelativityDestinationCaseGuid,
				_DESTINATION_WORKSPACE_INSTANCE_ID);
			return result;
		}

		private static bool IsFieldUpdateRequestValid(
			FieldUpdateRequestDto[] fieldUpdateRequestDtos,
			Guid fieldGuid,
			int expectedRelativityArtifactID)
		{
			FieldUpdateRequestDto jobHistoryGuidFieldUpdateRequestDto = fieldUpdateRequestDtos.Single(f => f.FieldIdentifier == fieldGuid);
			RelativityObjectRef[] relativityObjectReferences = ((RelativityObjectRef[])jobHistoryGuidFieldUpdateRequestDto.NewValue.Value);
			RelativityObjectRef relativityObjectRefArtifactID = relativityObjectReferences.Single();
			return relativityObjectRefArtifactID.ArtifactID == expectedRelativityArtifactID;

		}
	}
}
