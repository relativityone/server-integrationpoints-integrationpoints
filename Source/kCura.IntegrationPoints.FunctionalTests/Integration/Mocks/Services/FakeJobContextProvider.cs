using kCura.IntegrationPoints.Agent.Context;
using kCura.ScheduleQueue.Core;
using System;
using System.Reactive.Disposables;

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
