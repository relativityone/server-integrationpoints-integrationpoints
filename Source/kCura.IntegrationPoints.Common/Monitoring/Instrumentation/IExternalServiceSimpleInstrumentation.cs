using System;

namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation
{
	public interface IExternalServiceSimpleInstrumentation
	{
		T Execute<T>(Func<T> functionToExecute);
		void Execute(Action actionToExecute);
	}
}
