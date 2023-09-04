using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace Relativity.IntegrationPoints.Tests.Functional.CI.ApiTests
{
    internal class CustomProviderApiTests : TestsBase
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly RipApi _ripApi;

        public CustomProviderApiTests() : base(nameof(CustomProviderApiTests))
        {
            _serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;

            _ripApi = new RipApi(_serviceFactory);
        }

        public void OnSetupFixture()
        {
            var applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

            int appId = applicationService.InstallToLibrary(
                TestConfig.MyFirstProviderRapFileLocation,
                new LibraryApplicationInstallOptions { IgnoreVersion = true });

            applicationService.InstallToWorkspace(Workspace.ArtifactID, appId);
        }

        [Test]
        public async Task ImportEntity_UsingImportService_WhenAppendOnlyModeAndFullNameMapped()
        {
            // Arrange
            IntegrationPointModel integrationPoint = await CreateEntityImportIntegrationPointAsync(
                    "Entity Import with AppendOnly with FullName mapped",
                    ImportOverwriteModeEnum.AppendOnly)
                .ConfigureAwait(false);

            // Act

            // Assert
        }

        private async Task<IntegrationPointModel> CreateEntityImportIntegrationPointAsync(
            string name, ImportOverwriteModeEnum overwriteMode)
        {
            var commonDataSvc = new CommonIntegrationPointDataService(_serviceFactory, Workspace.ArtifactID);

            IntegrationPointModel integrationPoint = new IntegrationPointModel
            {
                Name = name,
                SourceProvider = await commonDataSvc.GetSourceProviderIdAsync(MyFirstProvider.Provider.GlobalConstants.FIRST_PROVIDER_GUID).ConfigureAwait(false),
                DestinationProvider = await commonDataSvc.GetDestinationProviderIdAsync(DestinationProviders.RELATIVITY).ConfigureAwait(false),
                Type = await commonDataSvc.GetIntegrationPointTypeByAsync(IntegrationPointTypes.ImportName).ConfigureAwait(false),

                LogErrors = true,
                ScheduleRule = new ScheduleModel(),

                OverwriteFieldsChoiceId = await commonDataSvc.GetOverwriteFieldsChoiceIdAsync(overwriteMode).ConfigureAwait(false),
                SourceConfiguration = null,
                SecuredConfiguration = null,
                DestinationConfiguration = null,
                FieldMappings = null
            };

            return integrationPoint;
        }
    }
}
