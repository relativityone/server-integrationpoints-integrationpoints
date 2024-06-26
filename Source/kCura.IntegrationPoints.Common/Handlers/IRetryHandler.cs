﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.Handlers
{
    public interface IRetryHandler
    {
        Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> function, [CallerMemberName] string callerName = "");

        Task ExecuteWithRetriesAsync(Func<Task> function, [CallerMemberName] string callerName = "");

        T ExecuteWithRetries<T>(Func<T> function, [CallerMemberName] string callerName = "");

        void ExecuteWithRetries(Action action, [CallerMemberName] string callerName = "");

        T Execute<T, TException>(Func<T> function, Action<TException> onRetry)
            where TException : Exception;
    }
}
