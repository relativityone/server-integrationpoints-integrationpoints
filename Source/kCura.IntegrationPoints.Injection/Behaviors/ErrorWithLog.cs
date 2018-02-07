using System;
using System.Collections.Generic;
using kCura.Injection;
using kCura.Injection.Behavior;

namespace kCura.IntegrationPoints.Injection.Behaviors
{
	public class ErrorWithLog : IBehavior
	{
		private readonly Error _error = new Error();

		public void Execute(kCura.Injection.Injection injection, IController injectionController)
		{
			Execute(injection, injectionController, new object[0]);
		}

		public void Execute(kCura.Injection.Injection injection, IController injectionController, IEnumerable<object> parameters)
		{
			string errorMessage = String.IsNullOrEmpty(injection.BehaviorData) ? " " : $" with the message '{injection.BehaviorData}' ";
			injectionController.Log(injection.InjectionPoint, $"Error Injection Point{errorMessage}is hit");
			_error.Execute(injection, injectionController, parameters);
		}
	}
}