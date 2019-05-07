using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class JobHistoryErrorTests
	{
		private const int _TEST_WORKSPACE_ARTIFACT_ID = 101789;

		private const int _TEST_ARTIFACT_ID = 101795;
		private const string _TEST_ERROR_MESSAGE = "Test error";
		private const ErrorType _TEST_ERROR_TYPE_ITEM = ErrorType.Item;
		private const ErrorType _TEST_ERROR_TYPE_JOB = ErrorType.Job;
		private const int _TEST_JOB_HISTORY_ARTIFACT_ID = 101800;
		private const string _TEST_SOURCE_UNIQUE_ID = "101810";
		private const string _TEST_STACK_TRACE = "Test stack trace.";

		private readonly Guid _expectedJobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");

		private readonly Guid _expectedErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private readonly Guid _expectedErrorStatusField = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");
		private readonly Guid _expectedErrorTypeField = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");
		private readonly Guid _expectedNameField = new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66");
		private readonly Guid _expectedSourceUniqueIdField = new Guid("5519435E-EE82-4820-9546-F1AF46121901");
		private readonly Guid _expectedStackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
		private readonly Guid _expectedTimestampUtcField = new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F");

		private readonly Guid _expectedErrorStatusNew = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A");

		private readonly Guid _expectedErrorTypeItem = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");
		private readonly Guid _expectedErrorTypeJob = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");

		[Test]
		public async Task CreateAsyncGoldFlowTest()
		{
			// Arrange
			var objectManager = new Mock<IObjectManager>();
			var serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).ReturnsAsync(new CreateResult
			{
				Object = new RelativityObject { ArtifactID = _TEST_ARTIFACT_ID }
			});

			CreateJobHistoryErrorDto expectedCreateJobHistoryErrorDto = CreateJobHistoryErrorDto(_TEST_ERROR_TYPE_JOB);

			// Act
			IJobHistoryError actualResult = await JobHistoryError.CreateAsync(serviceFactory.Object, _TEST_WORKSPACE_ARTIFACT_ID, expectedCreateJobHistoryErrorDto).ConfigureAwait(false);

			// Assert
			VerifyCreateJobHistoryErrorResult(actualResult, expectedCreateJobHistoryErrorDto);

			objectManager.Verify(x => x.CreateAsync(
				It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
				It.Is<CreateRequest>(y => VerifyCreateJobHistoryErrorRequest(y, expectedCreateJobHistoryErrorDto))));
		}

		[Test]
		public void CreateAsyncObjectManagerThrowsExceptionTest()
		{
			// Arrange
			var objectManager = new Mock<IObjectManager>();
			var serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<NotAuthorizedException>();

			CreateJobHistoryErrorDto expectedCreateJobHistoryErrorDto = CreateJobHistoryErrorDto(_TEST_ERROR_TYPE_ITEM);

			// Act & Assert
			Assert.ThrowsAsync<NotAuthorizedException>(async () =>
				await JobHistoryError.CreateAsync(serviceFactory.Object, _TEST_WORKSPACE_ARTIFACT_ID, expectedCreateJobHistoryErrorDto).ConfigureAwait(false));

			objectManager.Verify(x => x.CreateAsync(
				It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
				It.Is<CreateRequest>(y => VerifyCreateJobHistoryErrorRequest(y, expectedCreateJobHistoryErrorDto))));
		}

		private CreateJobHistoryErrorDto CreateJobHistoryErrorDto(ErrorType expectedErrorType)
		{
			var createJobHistoryErrorDto = new CreateJobHistoryErrorDto(_TEST_JOB_HISTORY_ARTIFACT_ID, expectedErrorType)
			{
				ErrorMessage = _TEST_ERROR_MESSAGE,
				SourceUniqueId = _TEST_SOURCE_UNIQUE_ID,
				StackTrace = _TEST_STACK_TRACE
			};
			return createJobHistoryErrorDto;
		}

		private void VerifyCreateJobHistoryErrorResult(IJobHistoryError actualResult, CreateJobHistoryErrorDto expectedCreateJobHistoryErrorDto)
		{
			Assert.IsNotNull(actualResult);
			Assert.AreEqual(_TEST_ARTIFACT_ID, actualResult.ArtifactId);
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.ErrorMessage, actualResult.ErrorMessage);
			Assert.AreEqual(ErrorStatus.New, actualResult.ErrorStatus);
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.ErrorType, actualResult.ErrorType);
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.JobHistoryArtifactId, actualResult.JobHistoryArtifactId);
			Assert.IsInstanceOf<Guid>(Guid.Parse(actualResult.Name));
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.SourceUniqueId, actualResult.SourceUniqueId);
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.StackTrace, actualResult.StackTrace);
			Assert.IsNotNull(actualResult.TimestampUtc);
		}

		private bool VerifyCreateJobHistoryErrorRequest(CreateRequest actualRequest, CreateJobHistoryErrorDto expectedCreateJobHistoryErrorDto)
		{
			Assert.IsNotNull(actualRequest);

			Assert.AreEqual(_expectedJobHistoryErrorObject, actualRequest.ObjectType.Guid);
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.JobHistoryArtifactId, actualRequest.ParentObject.ArtifactID);

			const int expectedNumberOfFields = 7;
			CollectionAssert.IsNotEmpty(actualRequest.FieldValues);

			IList<FieldRefValuePair> actualFieldValues = actualRequest.FieldValues.ToList();
			Assert.AreEqual(expectedNumberOfFields, actualFieldValues.Count);

			FieldRefValuePair actualErrorMessage = actualFieldValues.Single(x => x.Field.Guid == _expectedErrorMessageField);
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.ErrorMessage, actualErrorMessage.Value);

			FieldRefValuePair actualErrorStatus = actualFieldValues.Single(x => x.Field.Guid == _expectedErrorStatusField);
			Assert.IsInstanceOf<ChoiceRef>(actualErrorStatus.Value);
			Assert.AreEqual(_expectedErrorStatusNew, ((ChoiceRef)actualErrorStatus.Value).Guid);

			FieldRefValuePair actualName = actualFieldValues.Single(x => x.Field.Guid == _expectedNameField);
			Assert.IsInstanceOf<Guid>(Guid.Parse(actualName.Value.ToString()));

			FieldRefValuePair actualSourceUniqueId = actualFieldValues.Single(x => x.Field.Guid == _expectedSourceUniqueIdField);
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.SourceUniqueId, actualSourceUniqueId.Value);

			FieldRefValuePair actualStackTrace = actualFieldValues.Single(x => x.Field.Guid == _expectedStackTraceField);
			Assert.AreEqual(expectedCreateJobHistoryErrorDto.StackTrace, actualStackTrace.Value);

			FieldRefValuePair actualTimestamp = actualFieldValues.Single(x => x.Field.Guid == _expectedTimestampUtcField);
			Assert.IsInstanceOf<DateTime>(actualTimestamp.Value);

			FieldRefValuePair actualErrorType = actualFieldValues.Single(x => x.Field.Guid == _expectedErrorTypeField);
			Assert.IsInstanceOf<ChoiceRef>(actualErrorType.Value);
			var actualErrorTypeChoice = (ChoiceRef)actualErrorType.Value;

			switch (expectedCreateJobHistoryErrorDto.ErrorType)
			{
				case ErrorType.Job:
					Assert.AreEqual(_expectedErrorTypeJob, actualErrorTypeChoice.Guid);
					break;
				case ErrorType.Item:
					Assert.AreEqual(_expectedErrorTypeItem, actualErrorTypeChoice.Guid);
					break;
			}

			return true;
		}
	}
}