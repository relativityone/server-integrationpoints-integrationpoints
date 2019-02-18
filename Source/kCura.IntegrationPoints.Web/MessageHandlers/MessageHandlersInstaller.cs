using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace kCura.IntegrationPoints.Web.MessageHandlers
{
	public static class MessageHandlersInstaller
	{
		public static IWindsorContainer AddMessageHandler(this IWindsorContainer container)
		{
			container.Register(Component
				.For<CorrelationIdHandler>()
				.ImplementedBy<CorrelationIdHandler>()
			);

			return container;
		}
	}
}