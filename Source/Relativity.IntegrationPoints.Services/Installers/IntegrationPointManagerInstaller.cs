using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.IntegrationPoints.Data.Installers;
using Relativity.IntegrationPoints.Services.Installers.Context;
using Relativity.API;
using System.Collections.Generic;
using Relativity.IntegrationPoints.Services.Installers.Authentication;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;
using kCura.IntegrationPoints.RelativitySync;

namespace Relativity.IntegrationPoints.Services.Installers
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
                new ValidationInstaller(),
                new RelativitySyncInstaller(),
                new kCura.IntegrationPoints.ImportProvider.Parser.Installers.ServicesInstaller()
            };
        }

        protected override IList<IWindsorInstaller> Dependencies => _dependencies;

        protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
        {
            container.Register(Component.For<IUserInfo>().UsingFactoryMethod(k => k.Resolve<IServiceHelper>().GetAuthenticationManager().UserInfo, true));
            
            container.Register(Component.For<IIntegrationPointAccessor>().ImplementedBy<IntegrationPointAccessor>().LifestyleTransient());
            container.Register(Component.For<IIntegrationPointProfileAccessor>().ImplementedBy<IntegrationPointProfileAccessor>().LifestyleTransient());
            container.Register(Component.For<IProviderAccessor>().ImplementedBy<ProviderAccessor>().LifestyleTransient());
            container.Register(Component.For<IBackwardCompatibility>().ImplementedBy<BackwardCompatibility>().LifestyleTransient());
            container.Register(Component.For<IIntegrationPointRuntimeServiceFactory>().ImplementedBy<IntegrationPointRuntimeServiceFactory>().LifestyleTransient());

            container
                .AddWorkspaceContext(workspaceID)
                .AddAuthTokenGenerator();
        }
    }
}
