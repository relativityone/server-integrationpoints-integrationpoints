using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ProgressTests
	{
		private ProgressRepository _progressRepository;
		private Mock<IObjectManager> _objectManager;

		private const int _WORKSPACE_ID = 928;
		private const int _ARTIFACT_ID = 682;

		private const string _NAME_FIELD_NAME = "Name";
		private const string _PARENT_OBJECT_FIELD_NAME = "SyncConfiguration";

		private static readonly Guid ProgressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");

		private static readonly Guid OrderGuid = new Guid("610A1E44-7AAA-47FC-8FA0-92F8C8C8A94A");
		private static readonly Guid StatusGuid = new Guid("698E1BBE-13B7-445C-8A28-7D40FD232E1B");
		private static readonly Guid ExceptionGuid = new Guid("2F2CFC2B-C9C0-406D-BD90-FB0133BCB939");
		private static readonly Guid MessageGuid = new Guid("2E296F79-1B81-4BF6-98AD-68DA13F8DA44");

		[SetUp]
		public void SetUp()
		{
			Mock<ISourceServiceFactoryForAdmin> serviceFactoryForAdminMock = new Mock<ISourceServiceFactoryForAdmin>();
			_progressRepository = new ProgressRepository(serviceFactoryForAdminMock.Object, new EmptyLogger());

			_objectManager = new Mock<IObjectManager>();
			serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
		}

		[Test]
		public async Task ItShouldCreateProgress()
		{
			const int syncConfigurationArtifactId = 713;
			const string name = "name 1";
			const int order = 5;
			const SyncJobStatus status = SyncJobStatus.New;

			CreateResult result = new CreateResult
			{
				Object = new RelativityObject
				{
					ArtifactID = _ARTIFACT_ID
				}
			};
			_objectManager.Setup(x => x.CreateAsync(_WORKSPACE_ID, It.IsAny<CreateRequest>())).ReturnsAsync(result);

			// ACT
			IProgress progress = await _progressRepository.CreateAsync(_WORKSPACE_ID, syncConfigurationArtifactId, name, order, status).ConfigureAwait(false);

			// ASSERT
			progress.Name.Should().Be(name);
			progress.Order.Should().Be(order);
			progress.Status.Should().Be(status);
			progress.ArtifactId.Should().Be(_ARTIFACT_ID);

			_objectManager.Verify(x => x.CreateAsync(_WORKSPACE_ID, It.Is<CreateRequest>(cr => AssertCreateRequest(cr, name, order, status, syncConfigurationArtifactId))), Times.Once);
		}

		private bool AssertCreateRequest(CreateRequest createRequest, string name, int order, SyncJobStatus status, int syncConfigurationArtifactId)
		{
			createRequest.ObjectType.Guid.Should().Be(ProgressObjectTypeGuid);
			createRequest.ParentObject.ArtifactID.Should().Be(syncConfigurationArtifactId);
			const int three = 3;
			createRequest.FieldValues.Count().Should().Be(three);
			createRequest.FieldValues.Should().Contain(x => x.Field.Name == _NAME_FIELD_NAME);
			createRequest.FieldValues.First(x => x.Field.Name == _NAME_FIELD_NAME).Value.Should().Be(name);
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == OrderGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == OrderGuid).Value.Should().Be(order);

			string statusDescription = status.GetDescription();
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == StatusGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == StatusGuid).Value.Should().Be(statusDescription);
			return true;
		}

		[Test]
		public async Task ItShouldReadProgress()
		{
			const string name = "progress name";
			const int order = 2;
			const string statusDescription = "In Progress";
			const SyncJobStatus expectedStatus = SyncJobStatus.InProgress;
			const string exception = "exception 1";
			const string message = "message 1";

			QueryResult result = PrepareQueryResult(name, order, statusDescription, exception, message);
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(result);

			// ACT
			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ASSERT
			progress.ArtifactId.Should().Be(_ARTIFACT_ID);
			progress.Name.Should().Be(name);
			progress.Order.Should().Be(order);
			progress.Status.Should().Be(expectedStatus);
			progress.Exception.Should().Be(exception);
			progress.Message.Should().Be(message);

			_objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(queryRequest => AssertQueryRequest(queryRequest)), 0, 1), Times.Once);
		}

		[Test]
		public void ItShouldThrowWhenProgressNotFound()
		{
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			// ACT
			Func<Task> action = () => _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID);

			// ASSERT
			action.Should().Throw<SyncException>().Which.Message.Should().Be($"Progress ArtifactID: {_ARTIFACT_ID} not found.");
		}

		[Test]
		public async Task ItShouldQueryProgress()
		{
			const string name = "progress name 1";
			const int order = 3;
			const string statusDescription = "In Progress";
			const SyncJobStatus expectedStatus = SyncJobStatus.InProgress;
			const string exception = "exception 5";
			const string message = "message 9";
			const int syncConfigurationArtifactId = 854796;

			QueryResult result = PrepareQueryResult(name, order, statusDescription, exception, message);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			// ACT
			IProgress progress = await _progressRepository.QueryAsync(_WORKSPACE_ID, syncConfigurationArtifactId, name).ConfigureAwait(false);

			// ASSERT
			progress.ArtifactId.Should().Be(_ARTIFACT_ID);
			progress.Name.Should().Be(name);
			progress.Order.Should().Be(order);
			progress.Status.Should().Be(expectedStatus);
			progress.Exception.Should().Be(exception);
			progress.Message.Should().Be(message);

			_objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr, name, syncConfigurationArtifactId)), 1, 1), Times.Once);
		}

		private QueryResult PrepareQueryResult(string name = "name", int order = 1, string status = "Completed With Errors", string exception = "exception", string message = "message")
		{
			var queryResult = new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					PrepareRelativityObjectResult(name, order, status, exception, message)
				}
			};
			return queryResult;
		}

		private bool AssertQueryRequest(QueryRequest queryRequest, string name, int syncConfigurationArtifactId)
		{
			queryRequest.ObjectType.Guid.Should().Be(ProgressObjectTypeGuid);
			queryRequest.Condition.Should().Be($"'{_NAME_FIELD_NAME}' == '{name}' AND '{_PARENT_OBJECT_FIELD_NAME}' == {syncConfigurationArtifactId}");

			const int expectedNumberOfFields = 5;
			queryRequest.Fields.Count().Should().Be(expectedNumberOfFields);
			queryRequest.Fields.Should().Contain(x => x.Name == _NAME_FIELD_NAME);
			queryRequest.Fields.Should().Contain(x => x.Guid == ExceptionGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == OrderGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == StatusGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == MessageGuid);
			return true;
		}

		[Test]
		public async Task ItShouldHandleNullValues()
		{
			// only exception and message can be null
			// name and order are set during creation and cannot be modified
			// status is an enum and cannot be null
			const string exception = null;
			const string message = null;


			QueryResult result = PrepareQueryResult(string.Empty, 0, "Completed With Errors", exception, message);
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(result);

			// ACT
			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ASSERT
			progress.Exception.Should().Be(exception);
			progress.Message.Should().Be(message);
		}

		private static RelativityObject PrepareRelativityObjectResult(string name, int order, string status, string exception, string message)
		{
			var relativityObjectResult = new RelativityObject
			{
				ArtifactID = _ARTIFACT_ID,
				Name = name,
				FieldValues = new List<FieldValuePair>
				{
					new FieldValuePair
					{
						Field = new Field
						{
							Name = _NAME_FIELD_NAME
						},
						Value = name
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {OrderGuid}
						},
						Value = order
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {StatusGuid}
						},
						Value = status
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {ExceptionGuid}
						},
						Value = exception
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {MessageGuid}
						},
						Value = message
					}
				}
			};
			return relativityObjectResult;
		}

		private bool AssertQueryRequest(QueryRequest queryRequest)
		{
			queryRequest.Condition.Should().Be($"'ArtifactID' == {_ARTIFACT_ID}");
			const int five = 5;
			queryRequest.Fields.Count().Should().Be(five);
			queryRequest.Fields.Should().Contain(x => x.Name == _NAME_FIELD_NAME);
			queryRequest.Fields.Should().Contain(x => x.Guid == OrderGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == StatusGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == ExceptionGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == MessageGuid);
			return true;
		}

		[Test]
		public async Task ItShouldUpdateStatus()
		{
			const string statusDescription = "In Progress";
			const SyncJobStatus status = SyncJobStatus.InProgress;

			QueryResult result = PrepareQueryResult(string.Empty, 1, statusDescription, string.Empty, string.Empty);
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(result);

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await progress.SetStatusAsync(status).ConfigureAwait(false);

			// ASSERT
			progress.Status.Should().Be(status);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, StatusGuid, statusDescription))));
		}

		[Test]
		public async Task ItShouldNotSetStatusWhenUpdateFails()
		{
			const SyncJobStatus newValue = SyncJobStatus.CompletedWithErrors;

			QueryResult result = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(result);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			SyncJobStatus oldValue = progress.Status;

			// ACT
			Func<Task> action = () => progress.SetStatusAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			progress.Status.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == StatusGuid))), Times.Once);
		}

		[Test]
		public async Task ItShouldUpdateException()
		{
			InvalidOperationException exception = new InvalidOperationException();

			QueryResult result = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(result);

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await progress.SetExceptionAsync(exception).ConfigureAwait(false);

			// ASSERT
			progress.Exception.Should().Be(exception.ToString());

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, ExceptionGuid, exception.ToString()))));
		}

		[Test]
		public async Task ItShouldNotSetExceptionWhenUpdateFails()
		{
			InvalidOperationException newValue = new InvalidOperationException();

			QueryResult result = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(result);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			string oldValue = progress.Exception;

			// ACT
			Func<Task> action = () => progress.SetExceptionAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			progress.Exception.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == ExceptionGuid))), Times.Once);
		}

		[Test]
		public async Task ItShouldUpdateMessage()
		{
			const string message = "message 2";

			QueryResult result = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(result);

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await progress.SetMessageAsync(message).ConfigureAwait(false);

			// ASSERT
			progress.Message.Should().Be(message);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, MessageGuid, message))));
		}

		[Test]
		public async Task ItShouldNotSetMessageWhenUpdateFails()
		{
			const string newValue = "message 3";

			QueryResult result = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(result);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			string oldValue = progress.Message;

			// ACT
			Func<Task> action = () => progress.SetMessageAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			progress.Message.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == MessageGuid))), Times.Once);
		}

		private bool AssertUpdateRequest<T>(UpdateRequest updateRequest, Guid fieldGuid, T value)
		{
			updateRequest.Object.ArtifactID.Should().Be(_ARTIFACT_ID);
			updateRequest.FieldValues.Count().Should().Be(1);
			updateRequest.FieldValues.Should().Contain(x => x.Field.Guid == fieldGuid);
			updateRequest.FieldValues.Should().Contain(x => ((T) x.Value).Equals(value));
			return true;
		}

		[Test]
		public async Task ItShouldQueryAllProgressTests()
		{
			//Arrange
			const string name = "progress name 1";
			const int order = 3;
			const string statusDescription = "In Progress";
			const string exception = "exception 5";
			const string message = "message 10";
			const int syncConfigurationArtifactId = 854796;
			
			QueryResult queryResult1 = PrepareQueryResult(name, order, statusDescription, exception, message);
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, It.IsAny<int>())).ReturnsAsync(queryResult1);

			QueryResult queryResult2 = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult2);

			//Act
			IEnumerable<IProgress> progress = await _progressRepository.QueryAllAsync(_WORKSPACE_ID, syncConfigurationArtifactId)
				.ConfigureAwait(false);

			//Assert
			progress.Count().Should().Be(0);
			progress.Should().BeEmpty();

			_objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID,
					It.Is<QueryRequest>(qr => AssertQueryAllRequest(qr, syncConfigurationArtifactId)), 1,It.IsAny<int>()), Times.Once);
		}

		private bool AssertQueryAllRequest(QueryRequest queryRequest, int syncConfigurationArtifactId)
		{
			queryRequest.ObjectType.Guid.Should().Be(ProgressObjectTypeGuid);
			queryRequest.Condition.Should().Be($"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}");
			return true;
		}

		[Test]
		public async Task ItShouldQueryAllCompletedWithErrorsTests()
		{
			//Arrange
			const string name = "progress name 1";
			const int order = 3;
			const string statusDescription = "In Progress";
			const SyncJobStatus expectedStatus = SyncJobStatus.CompletedWithErrors;
			const string exception = "exception 5";
			const string message = "message 10";
			const int syncConfigurationArtifactId = 854796;

			QueryResult queryResult1 = PrepareQueryResult(name, order, statusDescription, exception, message);
			queryResult1.TotalCount = 1;
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, It.IsAny<int>())).ReturnsAsync(queryResult1);

			QueryResult queryResult2 = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult2);

			//Act
			IEnumerable<IProgress> progress = await _progressRepository.QueryAllAsync(_WORKSPACE_ID, syncConfigurationArtifactId)
				.ConfigureAwait(false);

			//Assert
			progress.First().Status.Should().Be(expectedStatus);
			progress.First().Exception.Should().Be("exception");
			progress.First().Message.Should().Be("message");
			progress.First().Name.Should().Be("name");
			progress.First().ArtifactId.Should().Be(_ARTIFACT_ID);
		}

		[Test]
		public async Task ItShouldQueryAllThrowExceptionTests()
		{
			//Arrange
			const string name = "progress name 1";
			const int order = 3;
			const string statusDescription = "In Progress";
			const string exception = "exception 5";
			const string message = "message 10";
			const int syncConfigurationArtifactId = 854796;

			QueryResult result = PrepareQueryResult(name, order, statusDescription, exception, message);
			result.TotalCount = 1;
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, It.IsAny<int>())).Throws<Exception>();

			//Act
			IEnumerable<IProgress> progress = await _progressRepository.QueryAllAsync(_WORKSPACE_ID, syncConfigurationArtifactId)
				.ConfigureAwait(false);

			//Assert
			progress.Count().Should().Be(0);
			progress.Should().BeEmpty();
		}
	}
}