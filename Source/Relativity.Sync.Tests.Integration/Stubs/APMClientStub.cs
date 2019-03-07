using System;
using System.Collections.Generic;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal sealed class APMClientStub : IAPMClient
	{
		public void Log(string name, Dictionary<string, object> data)
		{
			// Intentionally left empty
		}
	}
}
