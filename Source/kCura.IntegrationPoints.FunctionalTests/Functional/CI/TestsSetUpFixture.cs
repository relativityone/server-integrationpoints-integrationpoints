using Atata;
using System.IO;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Web;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Common.Extensions;

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

            Workspace workspace = RelativityFacade.Instance.CreateWorkspace(WORKSPACE_TEMPLATE_NAME);
            int workspaceId = workspace.ArtifactID;

            InstallIntegrationPointsToWorkspace(workspaceId);

            InstallARMTestServices();

            InstallDataTransferLegacy();

            workspace.InstallLegalHold();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            AtataContext.Current?.Dispose();
            CopyScreenshotsToBase();
        }

        private static bool TemplateWorkspaceExists()
            => RelativityFacade.Instance.Resolve<IWorkspaceService>().Get(WORKSPACE_TEMPLATE_NAME) != null;

        private static void InstallIntegrationPointsToWorkspace(int workspaceId)
        {
            string rapFileLocation = Path.Combine(TestContext.Parameters["RAPDirectory"], "kCura.IntegrationPoints.rap");

            var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

            int appId = applicationService.InstallToLibrary(rapFileLocation, 
                new LibraryApplicationInstallOptions { IgnoreVersion = true});

            applicationService.InstallToWorkspace(workspaceId, appId);
        }

        private static void InstallARMTestServices()
        {
            SetDevelopmentModeToTrue();

            RelativityFacade.Instance.Resolve<ILibraryApplicationService>()
                .InstallToLibrary(TestConfig.ARMTestServicesRapFileLocation, 
                    new LibraryApplicationInstallOptions { CreateIfMissing = true });
        }

        private static void SetDevelopmentModeToTrue()
        {
            RelativityFacade.Instance.Resolve<IInstanceSettingsService>()
                .Require(new Testing.Framework.Models.InstanceSetting
            {
                Name = "DevelopmentMode",
                Section = "kCura.ARM",
                Value = "True",
                ValueType = InstanceSettingValueType.TrueFalse
            }
            );
        }

        private static void InstallDataTransferLegacy()
        {
            RelativityFacade.Instance.Resolve<ILibraryApplicationService>()
                .InstallToLibrary(TestConfig.DataTransferLegacyRapFileLocation,
                    new LibraryApplicationInstallOptions { IgnoreVersion = true });
        }

        private static void CopyScreenshotsToBase()
        {
            string screenshotExtension = "*.png";
            string basePath = TestConfig.LogsDirectoryPath;

            try
            {
                string[] screenshotsPaths = Directory.GetFiles(basePath, screenshotExtension,
                    SearchOption.AllDirectories);
                TestContext.Progress.Log($"Found {screenshotsPaths.Length} screenshot(s)");
                
                foreach (var filePath in screenshotsPaths)
                {
                    string fileName = Path.GetFileName(filePath);
                    string directoryName = Directory.GetParent(fileName).Name;
                    string newFileName = $"{directoryName}_{fileName}";
                    TestContext.Progress.Log($"Copying screenshot: {filePath}");
                    string destFileName = Path.Combine(basePath, newFileName);
                    File.Copy(filePath, destFileName, true);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                TestContext.Progress.Log($"Could not found path with screenshots {basePath}", ex);
            }
        }
    }
}
