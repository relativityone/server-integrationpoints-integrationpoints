using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Moq;
using Moq.Language.Flow;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;

namespace Relativity.Sync.Tests.Integration
{
	internal static class MockExtensions
	{
		public static IReturnsResult<IObjectManager> SetupUpdateAsyncWithResult(this Mock<IObjectManager> mock, int workspaceId, Func<UpdateRequest, bool> requestMatcher, UpdateResult result)
		{
			return mock.Setup(x => x.UpdateAsync(
				workspaceId,
				It.Is<UpdateRequest>(r => requestMatcher(r)))
			).Returns(Task.FromResult(result));
		}

		public static IReturnsResult<IObjectManager> SetupQueryAsyncWithResult(this Mock<IObjectManager> objectManager, int workspaceId, QueryResult result)
		{
			return objectManager.Setup(x => x.QueryAsync(
				workspaceId,
				It.IsAny<QueryRequest>(),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(result));
		}

		public static IReturnsResult<IObjectManager> SetupQueryAsyncWithResult(this Mock<IObjectManager> objectManager, int workspaceId, Func<QueryRequest, bool> requestMatcher, QueryResult result)
		{
			return objectManager.Setup(x => x.QueryAsync(
				workspaceId,
				It.Is<QueryRequest>(r => requestMatcher(r)),
				It.IsAny<int>(),
				It.IsAny<int>(),
				CancellationToken.None,
				It.IsAny<IProgress<ProgressReport>>())
			).Returns(Task.FromResult(result));
		}
	}
}
