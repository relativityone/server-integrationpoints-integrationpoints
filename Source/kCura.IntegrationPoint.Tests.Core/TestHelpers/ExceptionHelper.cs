using System;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExceptionHelper
	{
		public static void IgnoreExceptions(Action action)
		{
			try
			{
				action();
			}
			catch
			{
			}
		}
	}
}