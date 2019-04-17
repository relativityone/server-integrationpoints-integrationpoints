using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Installers.Context;
using kCura.IntegrationPoints.Services.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using kCura.Relativity.Client;
using Relativity.API;
using System.Collections.Generic;
using kCura.IntegrationPoints.Services.Installers.Authentication;

namespace kCura.IntegrationPoints.Services.Installers
{
    public class IntegrationPointManagerInstaller : Installer
    {
        private readonly List<IWindsorInstaller> _dependencies;

        public IntegrationPointManagerInstaller()
        {
            _dependencies = new List<IWindsorInstaller>
            {
                new QueryInstallers(),
                new KeywordInstaller(),
                new SharedAgentInstaller(),
                new ServicesInstaller(),
                new ValidationInstaller()
            };
        }

        protected override IList<IWindsorInstaller> Dependencies => _dependencies;

        protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
        {
            container.Register(Component.For<IUserInfo>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>().GetAuthenticationManager().UserInfo, true));
            container.Register(Component.For<IRsapiClientWithWorkspaceFactory>().ImplementedBy<RsapiClientWithWorkspaceFactory>());
            
            container.Register(
                Component.For<IRSAPIClient>()
                    .UsingFactoryMethod(k =>
                    {
                        IAPILog logger = container.Resolve<IHelper>().GetLoggerFactory().GetLogger();
                        IRSAPIClient client = container.Resolve<IServiceHelper>().GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
                        client.APIOptions.WorkspaceID = workspaceID;
                        return new RsapiClientWrapperWithLogging(client, logger);
                    })
                    .LifeStyle.Transient);

            container.Register(Component.For<IIntegrationPointRepository>().ImplementedBy<IntegrationPointRepository>().LifestyleTransient());
            container.Register(Component.For<IIntegrationPointProfileRepository>().ImplementedBy<IntegrationPointProfileRepository>().LifestyleTransient());
            container.Register(Component.For<IProviderRepository>().ImplementedBy<ProviderRepository>().LifestyleTransient());
            container.Register(Component.For<IBackwardCompatibility>().ImplementedBy<BackwardCompatibility>().LifestyleTransient());
            container.Register(Component.For<IIntegrationPointRuntimeServiceFactory>().ImplementedBy<IntegrationPointRuntimeServiceFactory>().LifestyleTransient());

            container
                .AddWorkspaceContext(workspaceID)
                .AddAuthTokenGenerator();
        }
    }
}