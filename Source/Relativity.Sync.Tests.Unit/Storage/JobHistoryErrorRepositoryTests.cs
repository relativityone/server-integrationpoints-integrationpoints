using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	internal sealed class JobHistoryErrorRepositoryTests
	{
		private JobHistoryErrorRepository _jobHistoryErrorRepository;
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;
		private Mock<IObjectManager> _objectManager;
		private Mock<IDateTime> _dateTime;
		private DateTime _utcNow;

		private const ErrorType _TEST_ERROR_TYPE_ITEM = ErrorType.Item;

		private const int _TEST_JOB_HISTORY_ARTIFACT_ID = 101800;
		private const int _TEST_WORKSPACE_ARTIFACT_ID = 101789;
		private const string _TEST_ERROR_MESSAGE = "Test error";
		private const string _TEST_SOURCE_UNIQUE_ID = "101810";
		private const string _TEST_STACK_TRACE = "Test stack trace.";

		private readonly Guid _expectedErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private readonly Guid _expectedErrorStatusField = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");

		private readonly Guid _expectedErrorStatusNew = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A");
		private readonly Guid _expectedErrorTypeField = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");

		private readonly Guid _expectedErrorTypeItem = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");
		//private readonly Guid _expectedErrorTypeJob = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");

		//private readonly Guid _expectedJobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
		private readonly Guid _expectedNameField = new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66");
		private readonly Guid _expectedSourceUniqueIdField = new Guid("5519435E-EE82-4820-9546-F1AF46121901");
		private readonly Guid _expectedStackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
		private readonly Guid _expectedTimestampUtcField = new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F");

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_utcNow = DateTime.UtcNow;
			_dateTime = new Mock<IDateTime>();
			_dateTime.SetupGet(x => x.UtcNow).Returns(_utcNow);
			_objectManager = new Mock<IObjectManager>();
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_jobHistoryErrorRepository = new JobHistoryErrorRepository(_serviceFactory.Object, _dateTime.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldCreateJobHistoryError()
		{
			ErrorType errorType = ErrorType.Item;
			const string stackTrace = "Stack trace";
			const string sourceUniqueId = "src unique id";
			const string errorMessage = "Some message.";
			const int errorArtifactId = 10;
			CreateJobHistoryErrorDto createJobHistoryErrorDto = new CreateJobHistoryErrorDto(errorType)
			{
				ErrorMessage = errorMessage,
				SourceUniqueId = sourceUniqueId,
				StackTrace = stackTrace
			};
			MassCreateResult massCreateResult = new MassCreateResult()
			{
				Success = true,
				Objects = new List<RelativityObjectRef>()
				{
					new RelativityObjectRef()
					{
						ArtifactID = errorArtifactId
					}
				}
			};
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<MassCreateRequest>())).ReturnsAsync(massCreateResult);

			// act
			int createResult = await _jobHistoryErrorRepository.CreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, createJobHistoryErrorDto)
				.ConfigureAwait(false);

			// assert
			_objectManager.Verify(x => x.CreateAsync(It.IsAny<int>(), It.Is<MassCreateRequest>(cr => VerifyMassCreateRequest(cr, createJobHistoryErrorDto))));
			createResult.Should().Be(errorArtifactId);
		}

		[Test]
		public void CreateAsyncObjectManagerThrowsExceptionTest()
		{
			// Arrange
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<MassCreateRequest>())).Throws<NotAuthorizedException>();

			CreateJobHistoryErrorDto expectedCreateJobHistoryErrorDto = CreateJobHistoryErrorDto(_TEST_ERROR_TYPE_ITEM);

			// Act & Assert
			Assert.ThrowsAsync<NotAuthorizedException>(async () =>
				await _jobHistoryErrorRepository.MassCreateAsync(_TEST_WORKSPACE_ARTIFACT_ID, _TEST_JOB_HISTORY_ARTIFACT_ID, new List<CreateJobHistoryErrorDto>()
			{
				expectedCreateJobHistoryErrorDto
			}).ConfigureAwait(false));

			_objectManager.Verify(x => x.CreateAsync(
				It.Is<int>(y => y == _TEST_WORKSPACE_ARTIFACT_ID),
				It.Is<MassCreateRequest>(y => VerifyMassCreateRequest(y, expectedCreateJobHistoryErrorDto))));
		}

		private bool VerifyMassCreateRequest(MassCreateRequest request, CreateJobHistoryErrorDto dto)
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value
			IReadOnlyList<object> valueList = request.ValueLists[0];

			return request.Fields[0].Guid == _expectedErrorMessageField &&
				request.Fields[1].Guid == _expectedErrorStatusField &&
				request.Fields[2].Guid == _expectedErrorTypeField &&
				request.Fields[3].Guid == _expectedNameField &&
				request.Fields[4].Guid == _expectedSourceUniqueIdField &&
				request.Fields[5].Guid == _expectedStackTraceField &&
				request.Fields[6].Guid == _expectedTimestampUtcField &&
				
				(string)valueList[0] == dto.ErrorMessage &&
				((ChoiceRef)valueList[1]).Guid == _expectedErrorStatusNew &&
				((ChoiceRef)valueList[2]).Guid == _expectedErrorTypeItem &&
				(string)valueList[3] != Guid.Empty.ToString() &&
				(string)valueList[4] == dto.SourceUniqueId &&
				(string)valueList[5] == dto.StackTrace &&
				(DateTime)valueList[6] == _utcNow;
#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		private CreateJobHistoryErrorDto CreateJobHistoryErrorDto(ErrorType expectedErrorType)
		{
			var createJobHistoryErrorDto = new CreateJobHistoryErrorDto(expectedErrorType)
			{
				ErrorMessage = _TEST_ERROR_MESSAGE,
				SourceUniqueId = _TEST_SOURCE_UNIQUE_ID,
				StackTrace = _TEST_STACK_TRACE
			};
			return createJobHistoryErrorDto;
		}
	}
}