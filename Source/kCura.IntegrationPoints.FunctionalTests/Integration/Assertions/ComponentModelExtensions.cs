using Castle.Core;

namespace Relativity.IntegrationPoints.Tests.Integration.Assertions
{
    public static class ComponentModelExtensions
    {
        public static ComponentModelAssertions Should(this ComponentModel instance)
        {
            return new ComponentModelAssertions(instance);
        }
    }
}
