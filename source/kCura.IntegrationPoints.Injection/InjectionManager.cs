using kCura.Injection;
using System;
using System.Diagnostics;

namespace kCura.IntegrationPoints.Injection
{
	public class InjectionManager
	{
		private static readonly Lazy<InjectionManager> _instance 
			= new Lazy<InjectionManager>(() => new InjectionManager());
		private readonly Manager _manager;

		public static InjectionManager Instance => _instance.Value;

		protected InjectionManager()
		{
			_manager = new Manager();
			SetController(new InjectionController());
		}

		[Conditional("INJECTION")]
		public void Evaluate(string injectionPointId)
		{
			_manager.Evaluate(injectionPointId);
		}

		public void SetController(IController controller)
		{
			_manager.Controller = controller;
		}
	}
}
