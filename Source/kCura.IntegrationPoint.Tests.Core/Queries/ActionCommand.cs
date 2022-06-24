using kCura.IntegrationPoints.Data;
using System;

namespace kCura.IntegrationPoint.Tests.Core.Queries
{
    public class ActionCommand : ICommand
	{
		private readonly Action _action;

		public ActionCommand(Action action)
		{
			_action = action;
		}

		public static ActionCommand Empty => new ActionCommand(() => { });

		public void Execute()
		{
			_action();
		}
	}
}
