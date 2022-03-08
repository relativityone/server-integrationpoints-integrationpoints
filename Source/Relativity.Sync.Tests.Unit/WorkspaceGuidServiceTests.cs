using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class WorkspaceGuidServiceTests
	{
        private Mock<ISemaphoreSlim> _semaphoreSlim;
        private Mock<IObjectManager> _objectManager;
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdmin;
		private WorkspaceGuidService _instance;
        private Guid _workspaceGuid;

		[SetUp]
		public void SetUp()
		{
			_serviceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			_objectManager = new Mock<IObjectManager>();
			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManager.Object));
			_workspaceGuid = Guid.NewGuid();
            _semaphoreSlim = new Mock<ISemaphoreSlim>();
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						Guids = new List<Guid>()
						{
							_workspaceGuid
						}
					}
				}
			};
			_objectManager
				.Setup(x => x.QueryAsync(-1, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(queryResult)
				.Verifiable();
			_instance = new WorkspaceGuidService(_serviceFactoryForAdmin.Object, _semaphoreSlim.Object);
		}

		[Test]
		public async Task ItShouldQueryWorkspaceGuidWithObjectManager()
		{
			// act
			Guid guid = await _instance.GetWorkspaceGuidAsync(1).ConfigureAwait(false);

			// assert
			guid.Should().Be(_workspaceGuid);
			_objectManager.Verify();
		}

		[Test]
		public async Task ItShouldReturnWorkspaceGuidFromCache()
		{
			// act
			const int workspaceArtifactId = 1;
			Guid firstCallResult = await _instance.GetWorkspaceGuidAsync(workspaceArtifactId).ConfigureAwait(false);
			Guid secondCallResult = await _instance.GetWorkspaceGuidAsync(workspaceArtifactId).ConfigureAwait(false);

			// assert
			firstCallResult.Should().Be(_workspaceGuid);
			secondCallResult.Should().Be(_workspaceGuid);
			_objectManager.Verify(x => x.QueryAsync(-1, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
		}

		[Test]
		public void ItShouldThrowExceptionWhenWorkspaceNotFound()
		{
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(-1, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);

			// act
			Func<Task> action = async () => await _instance.GetWorkspaceGuidAsync(1).ConfigureAwait(false);

			// assert
			action.Should().Throw<NotFoundException>();
		}
	}
}