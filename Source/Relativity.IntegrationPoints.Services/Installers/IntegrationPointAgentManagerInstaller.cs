using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core.Data;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace Relativity.IntegrationPoints.Services.Installers
{
	public class IntegrationPointAgentManagerInstaller : Installer
	{
		protected override IList<IWindsorInstaller> Dependencies { get; } = new List<IWindsorInstaller>();

		protected override void RegisterComponents(IWindsorContainer container, IConfigurationStore store, int workspaceID)
		{
			Guid agentGuid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);
			container.Register(Component.For<IQueueQueryManager>().ImplementedBy<QueueQueryManager>().DependsOn(Dependency.OnValue<Guid>(agentGuid)).LifestyleTransient());
			container.Register(Component.For<IInstanceSettingsManager>().ImplementedBy<InstanceSettingsManager>().LifestyleTransient());
			container.Register(Component.For<IRepositoryFactory>().UsingFactoryMethod(c => new RepositoryFactory(c.Resolve<IHelper>(), c.Resolve<IHelper>().GetServicesManager())));
			container.Register(Component.For<IAPM>().UsingFactoryMethod(k => Client.APMClient, managedExternally: true).LifestyleSingleton());
		}
	}
}