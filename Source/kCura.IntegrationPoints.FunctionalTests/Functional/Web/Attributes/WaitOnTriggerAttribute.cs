using System.Threading;
using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Attributes
{
	internal class WaitOnTriggerAttribute : TriggerAttribute
	{
		private readonly int _waitSeconds;

		public WaitOnTriggerAttribute(int waitSeconds, TriggerEvents @on, TriggerPriority priority = TriggerPriority.Medium) : base(@on, priority)
		{
			_waitSeconds = waitSeconds;
		}

		protected override void Execute<TOwner>(TriggerContext<TOwner> context)
		{
			Thread.Sleep(_waitSeconds);
		}
	}
}
