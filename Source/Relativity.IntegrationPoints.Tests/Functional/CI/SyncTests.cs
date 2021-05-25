using System;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[IdentifiedTestFixture("74f17f40-697f-42b7-bad5-f83b8eeaa86a", Description = "RIP SYNC GOLD FLOWS")]
	[TestType.UI, TestType.MainFlow]
	public class SyncTests : TestsBase
	{
		[IdentifiedTest("b0afe8eb-e898-4763-9f95-e998f220b421")]
		public void SavedSearch_NativesAndMetadata_GoldFlow()
		{
			IWorkspaceService workspaceService = RelativityFacade.Instance.Resolve<IWorkspaceService>();

			Workspace workspace = new Workspace
			{
				Name = $"DUMMY_{Guid.NewGuid()}"
			};

			int workspaceId = workspaceService.Create(workspace).ArtifactID;

			DocumentListPage documentListPage = Being.On<DocumentListPage>(workspaceId);
		}
	}
}
