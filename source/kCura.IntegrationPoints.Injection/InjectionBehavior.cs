using System;
using kCura.Injection.Behavior;

namespace kCura.IntegrationPoints.Injection
{
	public class InjectionBehavior
	{
		public enum BehaviorType
		{
			Log = 1,
			Error = 2,
			InfiniteLoop = 3,
			Sleep = 4,
			PerformanceLog = 5,
			WaitUntil = 6
			// the rest of the injection behaviors are only supported in Relativity Core
		}

		public static BehaviorType GetTypeBasedOnBehavior(IBehavior injectionBehavior)
		{
			if (injectionBehavior is Log)
			{
				return BehaviorType.Log;
			}

			if (injectionBehavior is Error)
			{
				return BehaviorType.Error;
			}

			if (injectionBehavior is InfiniteLoop)
			{
				return BehaviorType.InfiniteLoop;
			}

			if (injectionBehavior is Sleep)
			{
				return BehaviorType.Sleep;
			}

			if (injectionBehavior is PerformanceLog)
			{
				return BehaviorType.PerformanceLog;
			}

			if (injectionBehavior is WaitUntil)
			{
				return BehaviorType.WaitUntil;
			}

			throw new Exception("Given injection IBehavior type is not defined as an enum.");
		} 
	}
}
