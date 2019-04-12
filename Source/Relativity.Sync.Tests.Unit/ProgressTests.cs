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

		private static readonly Guid ProgressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");

		private static readonly Guid OrderGuid = new Guid("610A1E44-7AAA-47FC-8FA0-92F8C8C8A94A");
		private static readonly Guid StatusGuid = new Guid("698E1BBE-13B7-445C-8A28-7D40FD232E1B");
		private static readonly Guid NameGuid = new Guid("AE2FCA2B-0E5C-4F35-948F-6C1654D5CF95");
		private static readonly Guid ExceptionGuid = new Guid("2F2CFC2B-C9C0-406D-BD90-FB0133BCB939");
		private static readonly Guid MessageGuid = new Guid("2E296F79-1B81-4BF6-98AD-68DA13F8DA44");
		private static readonly Guid ParentArtifactGuid = new Guid("E0188DD7-4B1B-454D-AFA4-3CCC7F9DC001");

		[SetUp]
		public void SetUp()
		{
			Mock<ISourceServiceFactoryForAdmin> serviceFactoryMock = new Mock<ISourceServiceFactoryForAdmin>();
			_progressRepository = new ProgressRepository(serviceFactoryMock.Object);

			_objectManager = new Mock<IObjectManager>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
		}

		[Test]
		public async Task ItShouldCreateProgress()
		{
			const int syncConfigurationArtifactId = 713;
			const string name = "name 1";
			const int order = 5;
			const string status = "pending";

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

		private bool AssertCreateRequest(CreateRequest createRequest, string name, int order, string status, int syncConfigurationArtifactId)
		{
			createRequest.ObjectType.Guid.Should().Be(ProgressObjectTypeGuid);
			createRequest.ParentObject.ArtifactID.Should().Be(syncConfigurationArtifactId);
			const int three = 3;
			createRequest.FieldValues.Count().Should().Be(three);
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == NameGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == NameGuid).Value.Should().Be(name);
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == OrderGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == OrderGuid).Value.Should().Be(order);
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == StatusGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == StatusGuid).Value.Should().Be(status);
			return true;
		}

		[Test]
		public async Task ItShouldReadProgress()
		{
			const string name = "progress name";
			const int order = 2;
			const string status = "status 1";
			const string exception = "exception 1";
			const string message = "message 1";

			ReadResult result = PrepareReadResult(name, order, status, exception, message);

			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(result);

			// ACT
			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ASSERT
			progress.ArtifactId.Should().Be(_ARTIFACT_ID);
			progress.Name.Should().Be(name);
			progress.Order.Should().Be(order);
			progress.Status.Should().Be(status);
			progress.Exception.Should().Be(exception);
			progress.Message.Should().Be(message);

			_objectManager.Verify(x => x.ReadAsync(_WORKSPACE_ID, It.Is<ReadRequest>(rr => AssertReadRequest(rr))), Times.Once);
		}

		[Test]
		public async Task ItShouldQueryProgress()
		{
			const string name = "progress name 1";
			const int order = 3;
			const string status = "status 3";
			const string exception = "exception 5";
			const string message = "message 9";
			const int syncConfigurationArtifactId = 854796;

			QueryResult result = PrepareQueryResult(order, status, exception, message);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			// ACT
			IProgress progress = await _progressRepository.QueryAsync(_WORKSPACE_ID, syncConfigurationArtifactId, name).ConfigureAwait(false);

			// ASSERT
			progress.ArtifactId.Should().Be(_ARTIFACT_ID);
			progress.Name.Should().Be(name);
			progress.Order.Should().Be(order);
			progress.Status.Should().Be(status);
			progress.Exception.Should().Be(exception);
			progress.Message.Should().Be(message);

			_objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr, name, syncConfigurationArtifactId)), 1, 1), Times.Once);
		}

		private QueryResult PrepareQueryResult(int order, string status, string exception, string message)
		{
			return new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						ArtifactID = _ARTIFACT_ID,
						FieldValues = new List<FieldValuePair>
						{
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
					}
				}
			};
		}

		private bool AssertQueryRequest(QueryRequest queryRequest, string name, int syncConfigurationArtifactId)
		{
			queryRequest.ObjectType.Guid.Should().Be(ProgressObjectTypeGuid);
			queryRequest.Condition.Should().Be($"'{NameGuid}' == '{name}' AND '{ParentArtifactGuid}' == {syncConfigurationArtifactId}");
			//TODO
			const int four = 4;
			queryRequest.Fields.Count().Should().Be(four);
			queryRequest.Fields.Should().Contain(x => x.Guid == ExceptionGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == OrderGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == StatusGuid);
			queryRequest.Fields.Should().Contain(x => x.Guid == MessageGuid);
			return true;
		}

		[Test]
		public async Task ItShouldHandleNullValues()
		{
			// only status, exception and message can be null - name and order are set during creation and cannot be modified
			const string status = null;
			const string exception = null;
			const string message = null;

			ReadResult result = PrepareReadResult(status: status, exception: exception, message: message);

			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(result);

			// ACT
			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ASSERT
			progress.Exception.Should().Be(exception);
			progress.Message.Should().Be(message);
		}

		private static ReadResult PrepareReadResult(string name = "name", int order = 1, string status = "status", string exception = "exception", string message = "message")
		{
			ReadResult readResult = new ReadResult
			{
				Object = new RelativityObject
				{
					ArtifactID = _ARTIFACT_ID,
					FieldValues = new List<FieldValuePair>
					{
						new FieldValuePair
						{
							Field = new Field
							{
								Guids = new List<Guid> {NameGuid}
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
				}
			};
			return readResult;
		}

		private bool AssertReadRequest(ReadRequest readRequest)
		{
			readRequest.Object.ArtifactID.Should().Be(_ARTIFACT_ID);
			const int five = 5;
			readRequest.Fields.Count().Should().Be(five);
			readRequest.Fields.Should().Contain(x => x.Guid == NameGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == OrderGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == StatusGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == ExceptionGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == MessageGuid);
			return true;
		}

		[Test]
		public async Task ItShouldUpdateStatus()
		{
			const string status = "status 2";

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await progress.SetStatusAsync(status).ConfigureAwait(false);

			// ASSERT
			progress.Status.Should().Be(status);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, StatusGuid, status))));
		}

		[Test]
		public async Task ItShouldNotSetStatusWhenUpdateFails()
		{
			const string newValue = "status 3";

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			string oldValue = progress.Status;

			// ACT
			Func<Task> action = async () => await progress.SetStatusAsync(newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			progress.Status.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == StatusGuid))), Times.Once);
		}

		[Test]
		public async Task ItShouldUpdateException()
		{
			InvalidOperationException exception = new InvalidOperationException();

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

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

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			string oldValue = progress.Exception;

			// ACT
			Func<Task> action = async () => await progress.SetExceptionAsync(newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			progress.Exception.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == ExceptionGuid))), Times.Once);
		}

		[Test]
		public async Task ItShouldUpdateMessage()
		{
			const string message = "message 2";

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

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

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IProgress progress = await _progressRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			string oldValue = progress.Message;

			// ACT
			Func<Task> action = async () => await progress.SetMessageAsync(newValue).ConfigureAwait(false);

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
		[TestCase(null)]
		[TestCase("")]
		public void ItShouldThrowOnEmptyName(string name)
		{
			const int order = 1;
			const string status = "status";

			// ACT
			Func<Task> action = async () => await _progressRepository.CreateAsync(_WORKSPACE_ID, 1, name, order, status).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
		}

		[Test]
		[TestCase(null)]
		[TestCase("")]
		public void ItShouldThrowOnEmptyStatus(string status)
		{
			const int order = 1;
			const string name = "name";

			// ACT
			Func<Task> action = async () => await _progressRepository.CreateAsync(_WORKSPACE_ID, 1, name, order, status).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
		}
	}
}