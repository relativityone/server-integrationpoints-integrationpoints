using System;
using kCura.Injection.Behavior;

namespace kCura.IntegrationPoints.Injection
{
	public class InjectionBehavior
	{
		public enum Type
		{
			Log = 1,
			Error = 2,
			InfiniteLoop = 3,
			Sleep = 4,
			PerformanceLog = 5,
			WaitUntil = 6
			// the rest of the injection behaviors are only supported in Relativity Core
		}

		public static Type GetTypeBasedOnBehavior(IBehavior injectionBehavior)
		{
			if (injectionBehavior is Log)
			{
				return Type.Log;
			}

			if (injectionBehavior is Error)
			{
				return Type.Error;
			}

			if (injectionBehavior is InfiniteLoop)
			{
				return Type.InfiniteLoop;
			}

			if (injectionBehavior is Sleep)
			{
				return Type.Sleep;
			}

			if (injectionBehavior is PerformanceLog)
			{
				return Type.PerformanceLog;
			}

			if (injectionBehavior is WaitUntil)
			{
				return Type.WaitUntil;
			}

			throw new Exception("Given injection IBehavior type is not defined as an enum.");
		} 
	}
}
