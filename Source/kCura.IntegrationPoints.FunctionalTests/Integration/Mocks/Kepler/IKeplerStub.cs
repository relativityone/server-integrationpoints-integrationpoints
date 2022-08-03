using System;
using Moq;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    interface IKeplerStub<T> where T : class, IDisposable
    {
        Mock<T> Mock { get; }
        T Object { get; }
    }
}
