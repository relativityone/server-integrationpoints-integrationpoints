using Atata;
using System.IO;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Web;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[SetUpFixture]
	public class TestsSetUpFixture
	{
		private const string STANDARD_ACCOUNT_EMAIL_FORMAT = "rip_func_user{0}@mail.com";

		public const string WORKSPACE_TEMPLATE_NAME = "RIP Functional Tests Template";

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			RelativityFacade.Instance.RelyOn<CoreComponent>();
			RelativityFacade.Instance.RelyOn<ApiComponent>();
			RelativityFacade.Instance.RelyOn<WebComponent>();

			RelativityFacade.Instance.Resolve<IAccountPoolService>().StandardAccountEmailFormat = STANDARD_ACCOUNT_EMAIL_FORMAT;

			if (TemplateWorkspaceExists())
			{
				return;
			}

			int workspaceId = RelativityFacade.Instance.CreateWorkspace(WORKSPACE_TEMPLATE_NAME).ArtifactID;

			InstallIntegrationPointsToWorkspace(workspaceId);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			AtataContext.Current?.Dispose();
		}

		private static bool TemplateWorkspaceExists()
			=> RelativityFacade.Instance.Resolve<IWorkspaceService>().Get(WORKSPACE_TEMPLATE_NAME) != null;

		private static void InstallIntegrationPointsToWorkspace(int workspaceId)
		{
			string rapFileLocation = Path.Combine(TestContext.Parameters["RAPDirectory"], "kCura.IntegrationPoints.rap");

			var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

			int appId = applicationService.InstallToLibrary(rapFileLocation, new LibraryApplicationInstallOptions
			{
				IgnoreVersion = true
			});

			applicationService.InstallToWorkspace(workspaceId, appId);
		}
	}
}
