using System;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public abstract class KeplerStubBase<T> : IKeplerStub<T>
        where T: class, IDisposable
    {
        protected RelativityInstanceTest Relativity { get; private set; }

        public Mock<T> Mock { get; }

        public T Object => Mock.Object;

        protected KeplerStubBase()
        {
            Mock = new Mock<T>();
        }

        public void Setup(RelativityInstanceTest relativity)
        {
            Relativity = relativity;
        }
    }
}
