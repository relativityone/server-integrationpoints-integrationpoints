using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Metrics
{
	public class RegisterSumMetricsCommandFactory : IRegisterSumMetricsCommandFactory
	{
		private readonly IEHHelper _helper;

		public RegisterSumMetricsCommandFactory(IEHHelper helper)
		{
			_helper = helper;
		}

		public IEHCommand CreateCommand<T>()
			where T: IEHCommand
		{
			if(typeof(T) == typeof(RegisterScheduledJobSumMetricsCommand))
			{
				return new RegisterScheduledJobSumMetricsCommand(_helper);
			}

			throw new InvalidOperationException($"Command {typeof(T)} has not been registered in factory");
		}
	}
}
