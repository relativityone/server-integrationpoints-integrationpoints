using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Installers;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Installer
{
    internal class AgentAggregatedInstaller : IWindsorInstaller
    {
        private readonly IAgentHelper _agentHelper;
        private readonly IScheduleRuleFactory _scheduleRuleFactory;

        public AgentAggregatedInstaller(IAgentHelper agentHelper, IScheduleRuleFactory scheduleRuleFactory)
        {
            _agentHelper = agentHelper;
            _scheduleRuleFactory = scheduleRuleFactory;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            try
            {
                container.Register(Component.For<IHelper>().Instance(_agentHelper));
                InstallContainer(container);
            }
            catch (Exception e)
            {
                IAPILog logger = _agentHelper.GetLoggerFactory().GetLogger().ForContext<AgentAggregatedInstaller>();
                logger.LogError(e, "Unable to install container using AgentAggregateInstaller");
                throw;
            }
        }

        private void InstallContainer(IWindsorContainer container)
        {
            container.Install(new InfrastructureInstaller());
            container.Install(new Data.Installers.QueryInstallers());
            container.Install(new KeywordInstaller());
            container.Install(new SharedAgentInstaller());
            container.Install(new ServicesInstaller());
            container.Install(new FilesDestinationProvider.Core.Installer.FileNamingInstaller());
            container.Install(new FilesDestinationProvider.Core.Installer.ExportInstaller());
            container.Install(new ImportProvider.Parser.Installers.ServicesInstaller());
            container.Install(new ValidationInstaller());
            container.Install(new AgentInstaller(_agentHelper, _scheduleRuleFactory));
        }
    }
}
