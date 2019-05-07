using System;
using System.Collections.Generic;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Logs APM metrics to Relativity APM.
	/// </summary>
	internal sealed class APMClient : IAPMClient
	{
		private readonly IAPM _apm;

		/// <summary>
		///     Creates an instance of <see cref="APMClient"/>.
		/// </summary>
		/// <param name="apm">Instance of <see cref="IAPM"/> instance to which this client forwards metrics</param>
		public APMClient(IAPM apm)
		{
			_apm = apm;
		}

		/// <inheritdoc />
		public void Log(string name, Dictionary<string, object> data)
		{
			_apm.CountOperation(name, customData: data).Write();
		}
	}
}
