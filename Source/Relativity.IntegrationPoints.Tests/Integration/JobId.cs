﻿namespace Relativity.IntegrationPoints.Tests.Integration
{
	public static class JobId
	{
		private static long _currentJobId = 0;

		public static long Next => ++_currentJobId;
	}
}
