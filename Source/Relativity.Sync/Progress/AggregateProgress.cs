using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Progress
{
    internal sealed class AggregateProgress<T> : IProgress<T>
    {
        private readonly IProgress<T>[] _progressReporters;

        public AggregateProgress(params IProgress<T>[] progressReporters)
        {
            _progressReporters = progressReporters;
        }

        public void Report(T value)
        {
            Parallel.ForEach(_progressReporters, p => p.Report(value));
        }
    }
}
