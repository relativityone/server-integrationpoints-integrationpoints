using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace kCura.IntegrationPoints.Domain.Logging
{
	public static class LogContextHelper
	{
		public static string AgentContextSlotName => "Rip.AgentCorrelationContext";
		public static string WebContextSlotName => "Rip.WebCorrelationContext";

		public static IDisposable CreateAgentLogContext(AgentCorrelationContext context)
		{
			var output = new DisposableLogContext(AgentContextSlotName);
			output.SetContext(context);
			return output;
		}

		public static AgentCorrelationContext GetAgentLogContext()
		{
			return GetCorrelationContext<AgentCorrelationContext>(AgentContextSlotName);
		}

		public static IDisposable CreateWebLogContext(WebCorrelationContext context)
		{
			var output = new DisposableLogContext(WebContextSlotName);
			output.SetContext(context);
			return output;
		}

		public static WebCorrelationContext GetWebLogContext()
		{
			return GetCorrelationContext<WebCorrelationContext>(WebContextSlotName);
		}

		private static T GetCorrelationContext<T>(string slotName) where T : BaseCorrelationContext, new()
		{
			var correlationContextDictionary = CallContext.LogicalGetData(slotName) as Dictionary<string, object>;
			if (correlationContextDictionary == null)
			{
				return null;
			}

			var agentCorrelationContext = new T();
			agentCorrelationContext.SetValuesFromDictionary(correlationContextDictionary);
			return agentCorrelationContext;
		}

		private class DisposableLogContext : IDisposable
		{
			private readonly string _slotName;

			public DisposableLogContext(string slotName)
			{
				_slotName = slotName;
			}

			public void SetContext(BaseCorrelationContext context)
			{
				CallContext.LogicalSetData(_slotName, context.ToDictionary());
			}

			public void Dispose()
			{
				CallContext.LogicalSetData(_slotName, null);
			}
		}
	}
}
