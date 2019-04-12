using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class ConfigurationTests : IDisposable
	{
		private ISourceServiceFactoryForAdmin _serviceFactory;
		private Mock<IObjectManager> _objectManager;
		private SemaphoreSlimStub _semaphoreSlim;

		private const int _WORKSPACE_ID = 458;
		private const int _ARTIFACT_ID = 365;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			Mock<ISourceServiceFactoryForAdmin> serviceFactoryMock = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_serviceFactory = serviceFactoryMock.Object;
		}

		[Test]
		public async Task ItShouldSuspendReadWhenUpdating()
		{
			Guid guid = Guid.NewGuid();
			const int initialValue = 100;
			const int newValue = 200;

			QueryResult result = PrepareQueryResult(guid, initialValue);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(result);

			const int second = 1000;
			_semaphoreSlim = new SemaphoreSlimStub(() => Thread.Sleep(second));

			SyncJobParameters jobParameters = new SyncJobParameters(_ARTIFACT_ID, _WORKSPACE_ID);
			IConfiguration cache = await Storage.Configuration.GetAsync(_serviceFactory, jobParameters, new EmptyLogger(), _semaphoreSlim).ConfigureAwait(false);

			// ACT
			Task updateTask = cache.UpdateFieldValueAsync(guid, newValue);
			int actualValue = cache.GetFieldValue<int>(guid);

			await updateTask.ConfigureAwait(false);

			// ASSERT
			actualValue.Should().Be(newValue);
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

		public void Dispose()
		{
			_semaphoreSlim?.Dispose();
		}
	}
}