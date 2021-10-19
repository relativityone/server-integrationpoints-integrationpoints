using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Installers;
using kCura.IntegrationPoints.Web.Installers.Context;
using kCura.IntegrationPoints.Web.Installers.IntegrationPointsServices;
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
using System.Web;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
    public class IntegrationPointProfilesControllerTests: TestsBase
    {
        private const string _LONG_TEXT_LIMIT_SECTION = "kCura.EDDS.Web";
        private const string _LONG_TEXT_LIMIT_NAME = "MaximumNumberOfCharactersSupportedByLongText";

        protected override WindsorContainer GetContainer()
        {
            var container = base.GetContainer();

            //RelativityServicesRegistration.AddRelativityServices(container);
            //IntegrationPointsServicesRegistration.AddIntegrationPointsServices(container);
            //ContextRegistration.AddContext(container);

            //HelpersRegistration.AddHelpers(container);
            //InfrastructureRegistration.AddInfrastructure(container);

            //ControllersRegistration.AddControllers(container);

            container.Kernel.ComponentModelCreated += model =>
            {
                if (model.LifestyleType == LifestyleType.PerWebRequest)
                {
                    model.LifestyleType = LifestyleType.Transient;
                }
            };

            //container.Register(Component.For<IntegrationPointProfilesAPIController>().ImplementedBy<IntegrationPointProfilesAPIController>().LifestyleTransient());
            return container;
        }

        [IdentifiedTest("d6cfade7-ccf0-4618-9173-4a52bb351172")]
        public void Method_shouldDoSth_When()
        {
            WorkspaceTest destinationWorkspace = FakeRelativityInstance.Helpers.WorkspaceHelper.CreateWorkspace();
            IntegrationPointProfileTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointProfileHelper.CreateSavedSearchIntegrationPoint(destinationWorkspace);
            IntegrationPointProfilesAPIController sut = Container.Resolve<IntegrationPointProfilesAPIController>();

            //var fakeRepository = Container.Resolve<IRepositoryFactory>();
            //IInstanceSettingRepository instanceSettings = fakeRepository.GetInstanceSettingRepository();
            //int longTextLimit = Convert.ToInt32(instanceSettings.GetConfigurationValue(_LONG_TEXT_LIMIT_SECTION, _LONG_TEXT_LIMIT_NAME));

        }

        //private IntegrationPointProfileModel CreateIntegrationPointProfile(ImportOverwriteModeEnum overwriteMode, string name, string overwrite)
        //{

        //}

    }
}
