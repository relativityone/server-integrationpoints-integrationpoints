﻿using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Interfaces;

namespace kCura.IntegrationPoints.Data
{
	internal class NonRetryHandler : IRetryHandler
	{
		public T ExecuteWithRetries<T>(Func<Task<T>> function, string callerName = "")
		{
			return function().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> function, string callerName = "")
		{
			return function();
		}
	}
}
