using Relativity.API;
using System;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Metrics
{
	public class RegisterSumMetricsCommandFactory : IRegisterSumMetricsCommandFactory
	{
		private readonly IEHHelper _helper;

		public RegisterSumMetricsCommandFactory(IEHHelper helper)
		{
			_helper = helper;
		}

		public IEHCommand CreateCommand<T>() where T: IEHCommand
		{
			if(typeof(T) == typeof(RegisterScheduleJobSumMetricsCommand))
			{
				return new RegisterScheduleJobSumMetricsCommand(_helper);
			}

			throw new InvalidOperationException($"Command {typeof(T)} has not been registered in factory");
		}
	}
}
