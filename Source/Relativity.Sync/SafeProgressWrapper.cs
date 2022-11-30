using System;
using Relativity.API;

namespace Relativity.Sync
{
    internal sealed class SafeProgressWrapper<T> : IProgress<T>
    {
        private readonly IProgress<T> _progress;
        private readonly IAPILog _logger;

        public SafeProgressWrapper(IProgress<T> progress, IAPILog logger)
        {
            _progress = progress;
            _logger = logger;
        }

        public void Report(T value)
        {
            try
            {
                _progress.Report(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    $"Received error when invoking externally-provided implementation of {typeof(IProgress<T>)} ({{type}})",
                    _progress.GetType().FullName);
            }
        }
    }
}
