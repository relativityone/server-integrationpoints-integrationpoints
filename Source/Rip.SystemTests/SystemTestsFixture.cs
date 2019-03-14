using NUnit.Framework;
using System;
using kCura.IntegrationPoint.Tests.Core;

namespace Rip.SystemTests
{
	[SetUpFixture]
	public class SystemTestsFixture
	{
		private const string _RELATIVITY_STARTER_TEMPLATE_NAME = "Relativity Starter Template";

		public static int WorkspaceID { get; private set; }

		[OneTimeSetUp]
		public void InitializeFixture()
		{
			WorkspaceID = Workspace.CreateWorkspace(
				workspaceName: $"CoreSearchManager-{DateTime.Now.Ticks}",
				templateName: _RELATIVITY_STARTER_TEMPLATE_NAME
			);
		}

		[OneTimeTearDown]
		public void TearDownFixture()
		{
			Workspace.DeleteWorkspace(WorkspaceID);
		}

	}
}
