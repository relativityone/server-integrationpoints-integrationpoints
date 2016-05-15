﻿using System;
using System.Threading;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Status
	{
		public static void WaitForProcessToComplete(IRSAPIClient rsapiClient, Guid processId, int timeout = 300, int interval = 500)
		{
			double timeWaitedInSeconds = 0.0;
			ProcessInformation processInfo = rsapiClient.GetProcessState(rsapiClient.APIOptions, processId);
			while (processInfo.State != ProcessStateValue.Completed)
			{
				if (timeWaitedInSeconds >= timeout)
				{
					throw new Exception(string.Format("Timed out waiting for Process: {0} to complete", processId));
				}

				if (processInfo.State == ProcessStateValue.CompletedWithError)
				{
					throw new Exception(string.Format("An error occurred while waiting on the Process {0} to complete. Error: {1}.", processId, processInfo.Message));
				}

				Thread.Sleep(interval);
				timeWaitedInSeconds += (interval / 1000.0);
				processInfo = rsapiClient.GetProcessState(rsapiClient.APIOptions, processId);
			}
		}
	}
}