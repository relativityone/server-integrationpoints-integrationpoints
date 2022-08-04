using System.Threading;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    public static class JobId
    {
        private static long _currentJobId = 0;

        public static long Next => Interlocked.Increment(ref _currentJobId);
    }
}
