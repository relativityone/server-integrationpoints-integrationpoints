using System;
using System.Runtime.Remoting.Messaging;

namespace kCura.IntegrationPoints.Domain.Logging
{
	public class SerilogContextRestorer : IDisposable
	{
		private readonly object _dataFromContext;

		public static string SerilogContextSlotName => "Serilog.Context.LogContext";

		public SerilogContextRestorer()
		{
			_dataFromContext = CallContext.LogicalGetData(SerilogContextSlotName);
		}

		public void Dispose()
		{
			CallContext.LogicalSetData(SerilogContextSlotName, _dataFromContext);
		}
	}
}
