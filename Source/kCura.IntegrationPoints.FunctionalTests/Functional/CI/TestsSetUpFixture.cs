﻿using System.IO;
using System.Threading.Tasks;
using Atata;
using kCura.IntegrationPoints.Common.Toggles;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Common.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
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
        public const string _WORKSPACE_TEMPLATE_NAME = "RIP Functional Tests Template";
        private const string STANDARD_ACCOUNT_EMAIL_FORMAT = "rip_func_user{0}@mail.com";
        private IToggleProviderExtended _toggleProvider;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            RelativityFacade.Instance.RelyOn<CoreComponent>();
            RelativityFacade.Instance.RelyOn<ApiComponent>();
            RelativityFacade.Instance.RelyOn<WebComponent>();

            RelativityFacade.Instance.Resolve<IAccountPoolService>().StandardAccountEmailFormat = STANDARD_ACCOUNT_EMAIL_FORMAT;

            _toggleProvider = SqlToggleProvider.Create();

            Workspace workspace = RequireTemplateWorkspace();
            int workspaceId = workspace.ArtifactID;

            await ConfigureTestingEnvironmentAsync().ConfigureAwait(false);

            InstallIntegrationPointsToWorkspace(workspaceId);

            InstallARMTestServices();

            InstallDataTransferLegacy();

            workspace.InstallLegalHold();
        }

        private async Task ConfigureTestingEnvironmentAsync()
        {
            ConfigureRelativityInstance();

            await SetTogglesAsync().ConfigureAwait(false);

            ConfigureHeap();
        }

        private void ConfigureHeap()
        {
            RelativityFacade.Instance.Resolve<IInstanceSettingsService>()
                .Require(new Testing.Framework.Models.InstanceSetting
                {
                    Name = "HeapEnvironmentId",
                    Section = "kCura.EDDS.Web",
                    Value = "1655229047",
                    ValueType = InstanceSettingValueType.Text
                });
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            AtataContext.Current?.Dispose();
            CopyScreenshotsToBase();
        }

        private static Workspace RequireTemplateWorkspace()
        {
            Workspace workspace = RelativityFacade.Instance.Resolve<IWorkspaceService>().Get(_WORKSPACE_TEMPLATE_NAME);
            if (workspace != null)
            {
                return workspace;
            }

            return RelativityFacade.Instance.CreateWorkspace(_WORKSPACE_TEMPLATE_NAME);
        }

        private void ConfigureRelativityInstance()
        {
            IInstanceSettingsService instanceSettings = RelativityFacade.Instance.Resolve<IInstanceSettingsService>();

            instanceSettings.UpdateValue(
                    "RelativityInstanceURL",
                    "Relativity.Core",
                    "https://localhost/Relativity");

            instanceSettings.UpdateValue(
                "DeveloperMode",
                "Relativity.Core",
                "True");
        }

        private async Task SetTogglesAsync()
        {
            await _toggleProvider.SetAsync<EnableSyncNonDocumentFlowToggle>(true).ConfigureAwait(false);
            await _toggleProvider.SetAsync<EnableTaggingToggle>(true).ConfigureAwait(false);

            await _toggleProvider.SetAsync("Relativity.Core.Toggle.EnableClickTracking", true);
        }

        private static void InstallIntegrationPointsToWorkspace(int workspaceId)
        {
            string rapFileLocation = Path.Combine(TestContext.Parameters["RAPDirectory"], "kCura.IntegrationPoints.rap");

            var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

            if (File.Exists(rapFileLocation))
            {
                int appId = applicationService.InstallToLibrary(
                    rapFileLocation,
                    new LibraryApplicationInstallOptions { IgnoreVersion = true });

                applicationService.InstallToWorkspace(workspaceId, appId);
            }
            else
            {
                var app = applicationService.Get(Const.Application.INTEGRATION_POINTS_APPLICATION_NAME);

                applicationService.InstallToWorkspace(workspaceId, app.ArtifactID);
            }
        }

        private static void InstallARMTestServices()
        {
            SetDevelopmentModeToTrue();

            var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

            if (applicationService.Get(Const.Application.ARM_TEST_SERVICES_APPLICATION_NAME) != null)
            {
                return;
            }

            applicationService.InstallToLibrary(
                TestConfig.ARMTestServicesRapFileLocation,
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
                });
        }

        private static void InstallDataTransferLegacy()
        {
            var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

            if (applicationService.Get(Const.Application.DATA_TRANSFER_LEGACY) != null)
            {
                return;
            }

            applicationService.InstallToLibrary(
                TestConfig.DataTransferLegacyRapFileLocation,
                new LibraryApplicationInstallOptions { IgnoreVersion = true });
        }

        private static void CopyScreenshotsToBase()
        {
            string screenshotExtension = "*.png";
            string basePath = TestConfig.LogsDirectoryPath;

            try
            {
                string[] screenshotsPaths = Directory.GetFiles(basePath, screenshotExtension, SearchOption.AllDirectories);
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
