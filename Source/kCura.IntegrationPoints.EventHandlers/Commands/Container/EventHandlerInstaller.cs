using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Container
{
	public class EventHandlerInstaller : IWindsorInstaller
	{
		private readonly IEHHelper _helper;

		public EventHandlerInstaller(IEHHelper helper)
		{
			_helper = helper;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
		}
	}
}