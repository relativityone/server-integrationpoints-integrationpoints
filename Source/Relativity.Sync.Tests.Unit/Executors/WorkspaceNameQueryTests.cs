using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class WorkspaceNameQueryTests
	{
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;

		private WorkspaceNameQuery _sut;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();

			_sut = new WorkspaceNameQuery(_serviceFactory.Object, new EmptyLogger());
		}

		[Test]
		public void ItShouldThrowExceptionWhenQueryFails()
		{
			Mock<IObjectManager> objectManager = new Mock<IObjectManager>();
			objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).Throws<InvalidOperationException>();
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			// act
			Func<Task> action = async () => await _sut.GetWorkspaceNameAsync(1, CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ItShouldThrowSyncExceptionWhenQueryReturnsNoResults()
		{
			Mock<IObjectManager> objectManager = new Mock<IObjectManager>();
			objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(new QueryResult());
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			// act
			Func<Task> action = async () => await _sut.GetWorkspaceNameAsync(1, CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncException>();
		}

		[Test]
		public async Task ItShouldReturnWorkspaceName()
		{
			string expectedWorkspaceName = "workspace name";
			QueryResult queryResult = new QueryResult();
			queryResult.Objects.Add(new RelativityObject()
			{
				Name = expectedWorkspaceName
			});

			Mock<IObjectManager> objectManager = new Mock<IObjectManager>();
			objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			// act
			string actualWorkspaceName = await _sut.GetWorkspaceNameAsync(1, CancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(expectedWorkspaceName, actualWorkspaceName);

		}
	}
}