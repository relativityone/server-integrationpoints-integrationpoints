using Castle.MicroKernel.Registration;
using Moq;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public static class WindsorContainerTestHelpers
    {
        public static ComponentRegistration<T> CreateDummyObjectRegistration<T>() where T : class
        {
            return Component.For<T>().Instance(new Mock<T>().Object);
        }
    }
}
