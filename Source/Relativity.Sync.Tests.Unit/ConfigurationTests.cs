using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ConfigurationTests
	{
		private Mock<IObjectManager> _objectManager;
		private Mock<ISemaphoreSlim> _semaphoreSlim;
		private Mock<ISourceServiceFactoryForAdmin> _sourceServiceFactoryForAdmin;

		private Guid _testFieldGuid;
		private SyncJobParameters _syncJobParameters;
		private ISyncLog _syncLog;

		private const int _TEST_FIELD_VALUE = 100;
		private const int _TEST_WORKSPACE_ID = 789;
		private const int _TEST_CONFIG_ARTIFACT_ID = 123;

		private static readonly Guid ConfigurationObjectTypeGuid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_testFieldGuid = Guid.NewGuid();
			_syncLog = new EmptyLogger();
			_syncJobParameters = new SyncJobParameters(_TEST_CONFIG_ARTIFACT_ID, _TEST_WORKSPACE_ID);
		}

		[SetUp]
		public void SetUp()
		{
			_semaphoreSlim = new Mock<ISemaphoreSlim>();
			_objectManager = new Mock<IObjectManager>();
			_sourceServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			_sourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
		}

		[Test]
		public async Task ItShouldReadFields()
		{
			// ARRANGE
			const int field1 = 456;
			Guid field1Guid = Guid.NewGuid();
			int? field2 = null;
			Guid field2Guid = Guid.NewGuid();

			var result = new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field
								{
									Guids = new List<Guid> {field1Guid}
								},
								Value = field1
							},
							new FieldValuePair
							{
								Field = new Field
								{
									Guids = new List<Guid> {field2Guid}
								},
								Value = field2
							}
						}
					}
				},
				TotalCount = 1
			};

			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			// ACT
			IConfiguration cache = await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

			// ASSERT
			cache.GetFieldValue<int>(field1Guid).Should().Be(field1);
			cache.GetFieldValue<int>(field2Guid).Should().Be(default(int));

			_objectManager.Verify(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr)), 1, 1), Times.Once);
		}

		[Test]
		[TestCase("")]
		[TestCase("test text")]
		public async Task ItShouldReadLongTextFieldsNotTruncatedWithoutKeplerStream(string testText)
		{
			// ARRANGE
			QueryResult result = BuildLongTextQueryResult(testText);

			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			// ACT
			IConfiguration cache = await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

			// ASSERT
			cache.GetFieldValue<string>(_testFieldGuid).Should().Be(testText);

			_objectManager.Verify(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr)), 1, 1), Times.Once);
		}

		[Test]
		public async Task ItShouldReadLongTextFieldsWithTruncationUsingKeplerStream()
		{
			// ARRANGE
			string testLongText = "this very long text...";
			string expectedLongText = "this very long text that is no longer truncated";
			QueryResult result = BuildLongTextQueryResult(testLongText);

			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			var testKeplerStream = new Mock<IKeplerStream>();

			_objectManager.Setup(x => x.StreamLongTextAsync(
					_TEST_WORKSPACE_ID,
					It.Is<RelativityObjectRef>(y => y.Guid == ConfigurationObjectTypeGuid && y.ArtifactID == _TEST_CONFIG_ARTIFACT_ID),
					It.Is<FieldRef>(y => y.Guid == _testFieldGuid))).ReturnsAsync(testKeplerStream.Object).Verifiable();

			var concreteStreamList = new List<Stream>();
			testKeplerStream.Setup(x => x.GetStreamAsync()).ReturnsAsync(() =>
			{
				byte[] text = System.Text.Encoding.Unicode.GetBytes(expectedLongText);
				var memoryStream = new MemoryStream(text);
				concreteStreamList.Add(memoryStream);
				return memoryStream;
			}).Verifiable();

			try
			{
				// ACT
				IConfiguration cache = await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

				// ASSERT
				Assert.IsNotEmpty(concreteStreamList);
				foreach (Stream stream in concreteStreamList)
				{
					// Verify all streams have been disposed
					Assert.IsFalse(stream.CanRead);
				}
				Assert.AreEqual(1, concreteStreamList.Count);

				cache.GetFieldValue<string>(_testFieldGuid).Should().Be(expectedLongText);

				Mock.Verify(_objectManager, testKeplerStream);
				_objectManager.Verify(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr)), 1, 1), Times.Once);
			}
			finally
			{
				foreach (Stream stream in concreteStreamList)
				{
					stream.Dispose();
				}
			}
		}

		[Test]
		public void ItShouldReadLongTextFieldsWithTruncationUsingKeplerStreamAndFailAfterThreeRetries()
		{
			// ARRANGE
			string testLongText = "this very long text...";
			QueryResult result = BuildLongTextQueryResult(testLongText);

			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			var testKeplerStream = new Mock<IKeplerStream>();

			_objectManager.Setup(x => x.StreamLongTextAsync(
				_TEST_WORKSPACE_ID,
				It.Is<RelativityObjectRef>(y => y.Guid == ConfigurationObjectTypeGuid && y.ArtifactID == _TEST_CONFIG_ARTIFACT_ID),
				It.Is<FieldRef>(y => y.Guid == _testFieldGuid))).ReturnsAsync(testKeplerStream.Object).Verifiable();

			testKeplerStream.Setup(x => x.GetStreamAsync()).Throws<IOException>();

			// ACT
			Assert.ThrowsAsync<IOException>(async () =>
				await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false));

			// ASSERT
			Mock.Verify(_objectManager, testKeplerStream);
			_objectManager.Verify(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr)), 1, 1), Times.Once);

			const int expectedNumberOfAttempts = 3;
			testKeplerStream.Verify(x => x.GetStreamAsync(), Times.Exactly(expectedNumberOfAttempts));
		}

		private QueryResult BuildLongTextQueryResult(string testLongText)
		{
			var result = new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field
								{
									FieldType = FieldType.LongText,
									Guids = new List<Guid> {_testFieldGuid}
								},
								Value = testLongText
							}
						}
					}
				},
				TotalCount = 1
			};
			return result;
		}

		private bool AssertQueryRequest(QueryRequest request)
		{
			request.ObjectType.Guid.Should().Be(ConfigurationObjectTypeGuid);
			request.Condition.Should().Be($"(('Artifact ID' == {_TEST_CONFIG_ARTIFACT_ID}))");
			request.Fields.First().Name.Should().Be("*");
			return true;
		}

		private QueryResult PrepareQueryResult(Guid guid, object value)
		{
			QueryResult result = new QueryResult
			{
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field
								{
									Guids = new List<Guid> {guid}
								},
								Value = value
							}
						}
					}
				},
				TotalCount = 1
			};
			return result;
		}

		[Test]
		public void ItShouldFailWhenConfigurationNotFound()
		{
			// ARRANGE
			var result = new QueryResult { TotalCount = 0 };
			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			// ACT
			Func<Task> action = async () => await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
		}

		[Test]
		public async Task ItShouldFailWhenReadingUnknownField()
		{
			// ARRANGE
			QueryResult result = PrepareQueryResult(_testFieldGuid, _TEST_FIELD_VALUE);
			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			IConfiguration cache = await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

			// ACT
			Action action = () => cache.GetFieldValue<int>(Guid.NewGuid());

			// ASSERT
			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public async Task ItShouldFailWhenUpdatingUnknownField()
		{
			// ARRANGE
			QueryResult result = PrepareQueryResult(_testFieldGuid, _TEST_FIELD_VALUE);
			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			IConfiguration cache = await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

			// ACT
			Func<Task> action = async () => await cache.UpdateFieldValueAsync(Guid.NewGuid(), 0).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public async Task ItShouldUpdateField()
		{
			// ARRANGE
			const int newValue = 200;
			QueryResult result = PrepareQueryResult(_testFieldGuid, _TEST_FIELD_VALUE);
			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			IConfiguration cache = await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

			// ACT
			await cache.UpdateFieldValueAsync(_testFieldGuid, newValue).ConfigureAwait(false);

			// ASSERT
			cache.GetFieldValue<int>(_testFieldGuid).Should().Be(newValue);

			_objectManager.Verify(x => x.UpdateAsync(_TEST_WORKSPACE_ID, It.Is<UpdateRequest>(ur => AssertUpdateRequest(ur, _testFieldGuid, newValue))));
		}

		private bool AssertUpdateRequest(UpdateRequest updateRequest, Guid guid, int value)
		{
			updateRequest.Object.ArtifactID.Should().Be(_TEST_CONFIG_ARTIFACT_ID);
			updateRequest.FieldValues.Count().Should().Be(1);
			updateRequest.FieldValues.Should().Contain(x => x.Field.Guid == guid);
			updateRequest.FieldValues.First(x => x.Field.Guid == guid).Value.Should().Be(value);
			return true;
		}

		[Test]
		public async Task ItShouldNotSetNewValueWhenUpdateFails()
		{
			// ARRANGE
			const int newValue = 200;
			QueryResult result = PrepareQueryResult(_testFieldGuid, _TEST_FIELD_VALUE);

			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);
			_objectManager.Setup(x => x.UpdateAsync(_TEST_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<InvalidOperationException>();

			IConfiguration cache = await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

			// ACT
			Func<Task> action = async () => await cache.UpdateFieldValueAsync(_testFieldGuid, newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<InvalidOperationException>();

			cache.GetFieldValue<int>(_testFieldGuid).Should().Be(_TEST_FIELD_VALUE);
		}

		[Test]
		public async Task ItShouldDisposeSemaphore()
		{
			// ARRANGE
			QueryResult result = PrepareQueryResult(_testFieldGuid, _TEST_FIELD_VALUE);
			_objectManager.Setup(x => x.QueryAsync(_TEST_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			IConfiguration cache = await Storage.Configuration.GetAsync(_sourceServiceFactoryForAdmin.Object, _syncJobParameters, _syncLog, _semaphoreSlim.Object).ConfigureAwait(false);

			// ACT
			cache.Dispose();

			// ASSERT
			_semaphoreSlim.Verify(x => x.Dispose(), Times.Once);
		}
	}
}