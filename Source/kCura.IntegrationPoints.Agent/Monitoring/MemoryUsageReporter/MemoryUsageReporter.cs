using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
	public class MemoryUsageReporter : IMemoryUsageReporter
	{
		private IAPM _apmClient;
		private Timer _timerThread;
		private long _jobId;
		private string _jobType;

		public MemoryUsageReporter(IAPM apmClient)
		{
			_timerThread = new Timer(state => Execute(), null, Timeout.Infinite, Timeout.Infinite);
			_apmClient = apmClient;
		}

		public IDisposable ActivateTimer(int timeInterval, long jobId, string jobType)
		{
			SetJobData(jobId, jobType);
			_timerThread.Change(0, timeInterval);
			return _timerThread;
		}

		private void SetJobData(long jobId, string jobType)
        {
			_jobId = jobId;
			_jobType = jobType;
        }

		private void Execute()
		{
			long memoryUsage = ProcessMemoryHelper.GetCurrentProcessMemoryUsage();

			Dictionary<string, object> customData = new Dictionary<string, object>() 
			{
				{"Name", "IntegrationPoints.Performance.System" },
				{ "MemoryUsage", memoryUsage },
				{ "JobId", _jobId },
				{ "JobType", _jobType }
			};

			_apmClient.CountOperation("Relativity.IntegrationPoints.Performance", customData: customData).Write();
		}
	}
}
