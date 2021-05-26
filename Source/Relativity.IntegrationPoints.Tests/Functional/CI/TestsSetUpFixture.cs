using Atata;
using System.IO;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[SetUpFixture]
	public class TestsSetUpFixture
	{
		private const string FUNCTIONAL_TEMPLATE_NAME = "RIP Functional Tests Template";
		private const string FUNCTIONAL_STANDARD_ACCOUNT_EMAIL_FORMAT = "rip_func_user{0}@mail.com";

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			RelativityFacade.Instance.RelyOn<CoreComponent>();
			RelativityFacade.Instance.RelyOn<ApiComponent>();
			RelativityFacade.Instance.RelyOn<WebComponent>();

			RelativityFacade.Instance.Resolve<IAccountPoolService>().StandardAccountEmailFormat = FUNCTIONAL_STANDARD_ACCOUNT_EMAIL_FORMAT;

			if (TemplateWorkspaceExists())
			{
				return;
			}

			int workspaceId = CreateTemplateWorkspace();

			InstallIntegrationPointsToWorkspace(workspaceId);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			AtataContext.Current?.Dispose();
		}

		private static bool TemplateWorkspaceExists()
			=> RelativityFacade.Instance.Resolve<IWorkspaceService>().Get(FUNCTIONAL_TEMPLATE_NAME) != null;

		private static int CreateTemplateWorkspace()
		{
			Workspace newWorkspace = new Workspace()
			{
				Name = FUNCTIONAL_TEMPLATE_NAME
			};

			return RelativityFacade.Instance.Resolve<IWorkspaceService>().Create(newWorkspace).ArtifactID;
		}

		private static void InstallIntegrationPointsToWorkspace(int workspaceId)
		{
			string rapFileLocation = Path.Combine(TestContext.Parameters["RAPDirectory"], "RelativityIntegrationPoints.Auto.rap");

			var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

			int appId = applicationService.InstallToLibrary(rapFileLocation, new LibraryApplicationInstallOptions
			{
				IgnoreVersion = true
			});

			applicationService.InstallToWorkspace(workspaceId, appId);
		}
	}
}
