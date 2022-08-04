using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions.Assertions;

namespace kCura.IntegrationPoint.Tests.Core.FluentAssertions
{
    public static class WindsorContainerExtensions
    {
        public static WindsorContainerAssertions Should(this IWindsorContainer instace)
        {
            return new WindsorContainerAssertions(instace);
        }
    }
}
