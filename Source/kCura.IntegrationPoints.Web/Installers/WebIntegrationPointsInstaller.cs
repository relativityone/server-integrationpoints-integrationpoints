using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Web.Installers.Context;
using kCura.IntegrationPoints.Web.Installers.IntegrationPointsServices;

namespace kCura.IntegrationPoints.Web.Installers
{
    public class WebIntegrationPointsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .ConfigureContainer()
                .AddRelativityServices()
                .AddIntegrationPointsServices()
                .AddContext()
                .AddHelpers()
                .AddInfrastructure()
                .AddControllers();
        }
    }
}
