using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Exceptions;
using NUnit.Framework;
using System;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceContext
{
	[TestFixture]
	public class LastWorkspaceContextServiceTests
	{
		[Test]
		public void GetWorkspaceId_ShouldThrowException()
		{
			// arrange
			var sut = new LastWorkspaceContextService();
			Action getWorkspaceIdAction = () => sut.GetWorkspaceId();

			// act & assert
			getWorkspaceIdAction.ShouldThrow<WorkspaceIdNotFoundException>();
		}
	}
}
