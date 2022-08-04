using Castle.Core;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions.Assertions;

namespace kCura.IntegrationPoint.Tests.Core.FluentAssertions
{
    public static class ComponentModelExtensions
    {
        public static ComponentModelAssertions Should(this ComponentModel instace)
        {
            return new ComponentModelAssertions(instace);
        }
    }
}
