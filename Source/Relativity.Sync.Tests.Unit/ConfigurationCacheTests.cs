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
	public sealed class ConfigurationCacheTests
	{
		private ISourceServiceFactoryForAdmin _serviceFactory;
		private Mock<IObjectManager> _objectManager;

		private const int _WORKSPACE_ID = 789;
		private const int _ARTIFACT_ID = 123;

		private static readonly Guid ConfigurationObjectTypeGuid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			var serviceFactoryMock = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_serviceFactory = serviceFactoryMock.Object;
		}

		[Test]
		public async Task ItShouldReadFields()
		{
			const int field1 = 456;
			Guid field1Guid = Guid.NewGuid();
			int? field2 = null;
			Guid field2Guid = Guid.NewGuid();

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

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			// ACT
			IConfiguration cache = await Storage.Configuration.GetAsync(_serviceFactory, _WORKSPACE_ID, _ARTIFACT_ID, new EmptyLogger(), Mock.Of<ISemaphoreSlim>()).ConfigureAwait(false);

			// ASSERT
			cache.GetFieldValue<int>(field1Guid).Should().Be(field1);
			cache.GetFieldValue<int>(field2Guid).Should().Be(default(int));

			_objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr)), 1, 1), Times.Once);
		}

		private bool AssertQueryRequest(QueryRequest request)
		{
			request.ObjectType.Guid.Should().Be(ConfigurationObjectTypeGuid);
			request.Condition.Should().Be($"(('Artifact ID' == {_ARTIFACT_ID}))");
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
			QueryResult result = new QueryResult
			{
				TotalCount = 0
			};

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			// ACT
			Func<Task> action = async () => await Storage.Configuration.GetAsync(_serviceFactory, _WORKSPACE_ID, _ARTIFACT_ID, new EmptyLogger(), Mock.Of<ISemaphoreSlim>()).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
		}

		[Test]
		public async Task ItShouldFailWhenReadingUnknownField()
		{
			Guid guid = Guid.NewGuid();
			const int value = 100;

			QueryResult result = PrepareQueryResult(guid, value);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			IConfiguration cache = await Storage.Configuration.GetAsync(_serviceFactory, _WORKSPACE_ID, _ARTIFACT_ID, new EmptyLogger(), Mock.Of<ISemaphoreSlim>()).ConfigureAwait(false);

			// ACT
			Action action = () => cache.GetFieldValue<int>(Guid.NewGuid());

			// ASSERT
			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public async Task ItShouldFailWhenUpdatingUnknownField()
		{
			Guid guid = Guid.NewGuid();
			const int value = 100;

			QueryResult result = PrepareQueryResult(guid, value);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			IConfiguration cache = await Storage.Configuration.GetAsync(_serviceFactory, _WORKSPACE_ID, _ARTIFACT_ID, new EmptyLogger(), Mock.Of<ISemaphoreSlim>()).ConfigureAwait(false);

			// ACT
			Func<Task> action = async () => await cache.UpdateFieldValueAsync(Guid.NewGuid(), 0).ConfigureAwait(false);
			
			// ASSERT
			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public async Task ItShouldUpdateField()
		{
			Guid guid = Guid.NewGuid();
			const int initialValue = 100;
			const int newValue = 200;

			QueryResult result = PrepareQueryResult(guid, initialValue);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			IConfiguration cache = await Storage.Configuration.GetAsync(_serviceFactory, _WORKSPACE_ID, _ARTIFACT_ID, new EmptyLogger(), Mock.Of<ISemaphoreSlim>()).ConfigureAwait(false);

			// ACT
			await cache.UpdateFieldValueAsync(guid, newValue).ConfigureAwait(false);

			// ASSERT
			cache.GetFieldValue<int>(guid).Should().Be(newValue);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(ur => AssertUpdateRequest(ur, guid, newValue))));
		}

		private bool AssertUpdateRequest(UpdateRequest updateRequest, Guid guid, int value)
		{
			updateRequest.Object.ArtifactID.Should().Be(_ARTIFACT_ID);
			updateRequest.FieldValues.Count().Should().Be(1);
			updateRequest.FieldValues.Should().Contain(x => x.Field.Guid == guid);
			updateRequest.FieldValues.First(x => x.Field.Guid == guid).Value.Should().Be(value);
			return true;
		}

		[Test]
		public async Task ItShouldNotSetNewValueWhenUpdateFails()
		{
			Guid guid = Guid.NewGuid();
			const int initialValue = 100;
			const int newValue = 200;

			QueryResult result = PrepareQueryResult(guid, initialValue);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<InvalidOperationException>();

			IConfiguration cache = await Storage.Configuration.GetAsync(_serviceFactory, _WORKSPACE_ID, _ARTIFACT_ID, new EmptyLogger(), Mock.Of<ISemaphoreSlim>()).ConfigureAwait(false);

			// ACT
			Func<Task> action = async () => await cache.UpdateFieldValueAsync(guid, newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<InvalidOperationException>();

			cache.GetFieldValue<int>(guid).Should().Be(initialValue);
		}

		[Test]
		public async Task ItShouldDisposeSemaphore()
		{
			Guid guid = Guid.NewGuid();
			const int value = 100;

			QueryResult result = PrepareQueryResult(guid, value);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			Mock<ISemaphoreSlim> semaphore = new Mock<ISemaphoreSlim>();

			IConfiguration cache = await Storage.Configuration.GetAsync(_serviceFactory, _WORKSPACE_ID, _ARTIFACT_ID, new EmptyLogger(), semaphore.Object).ConfigureAwait(false);

			// ACT
			cache.Dispose();

			// ASSERT
			semaphore.Verify(x => x.Dispose(), Times.Once);
		}
	}
}