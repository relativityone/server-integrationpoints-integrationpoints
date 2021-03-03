using System;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public static class Artifact
	{
		private static readonly Random _random;

		private const int _MIN_VALUE = 100000;
		private const int _MAX_VALUE = 999999;

		static Artifact()
		{
			_random = new Random();
		}

		public static int NextId()
		{
			return _random.Next(_MIN_VALUE, _MAX_VALUE);
		}
	}
}
