using System;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;

namespace kCura.IntegrationPoints.Core.Monitoring.Instrumentation
{
	internal class ExternalServiceSimpleInstrumentation : IExternalServiceSimpleInstrumentation 
	{
		private readonly IExternalServiceInstrumentation _instrumentation;

		public ExternalServiceSimpleInstrumentation(IExternalServiceInstrumentation instrumentation)
		{
			_instrumentation = instrumentation;
		}

		public T Execute<T>(Func<T> functionToExecute)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = _instrumentation.Started();
			try
			{
				T result = functionToExecute();
				startedInstrumentation.Completed();
				return result;
			}
			catch (Exception ex)
			{
				startedInstrumentation.Failed(ex);
				throw;
			}
		}

		public void Execute(Action actionToExecute)
		{
			IExternalServiceInstrumentationStarted startedInstrumentation = _instrumentation.Started();
			try
			{
				actionToExecute();
				startedInstrumentation.Completed();
			}
			catch (Exception ex)
			{
				startedInstrumentation.Failed(ex);
				throw;
			}
		}
	}
}
