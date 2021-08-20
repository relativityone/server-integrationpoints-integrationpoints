using Castle.Windsor;

namespace Relativity.IntegrationPoints.Tests.Integration.Assertions
{
	public static class WindsorContainerExtensions
	{
		public static WindsorContainerAssertions Should(this IWindsorContainer instance)
		{
			return new WindsorContainerAssertions(instance);
		}
	}
}
