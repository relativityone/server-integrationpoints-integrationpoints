using kCura.IntegrationPoints.Agent.Context;
using System;
using System.Reactive.Disposables;
using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeJobContextProvider : IJobContextProvider
    {
        public Job Job { get; private set; }

        public IDisposable StartJobContext(Job job)
        {
            Job = job;

            return Disposable.Create(() => Job = null);
        }
    }
}
