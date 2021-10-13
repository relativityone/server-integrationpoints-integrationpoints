using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers;
using kCura.IntegrationPoints.Web.Models;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class IntegrationPointProfilesControllerTests: TestsBase
    {
        private const string _LONG_TEXT_LIMIT_SECTION = "kCura.EDDS.Web";
        private const string _LONG_TEXT_LIMIT_NAME = "MaximumNumberOfCharactersSupportedByLongText";

        protected override WindsorContainer GetContainer()
        {
            var container = base.GetContainer();

            container.Register(Component.For<IntegrationPointProfilesController>().ImplementedBy<IntegrationPointProfilesController>());

            return container;
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Method_shouldDoSth_When()
        {
            var container = GetContainer();
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPoint(destinationWorkspace);
            IntegrationPointProfilesController sut = container.Resolve<IntegrationPointProfilesController>();
            FakeRepositoryFactory fakeRepository = container.Resolve<FakeRepositoryFactory>();
            IInstanceSettingRepository instanceSettings = fakeRepository.GetInstanceSettingRepository();
            int longTextLimit = Convert.ToInt32(instanceSettings.GetConfigurationValue(_LONG_TEXT_LIMIT_SECTION, _LONG_TEXT_LIMIT_NAME));

        }

        //private IntegrationPointProfileModel CreateIntegrationPointProfile(ImportOverwriteModeEnum overwriteMode, string name, string overwrite)
        //{

        //}

    }
}
