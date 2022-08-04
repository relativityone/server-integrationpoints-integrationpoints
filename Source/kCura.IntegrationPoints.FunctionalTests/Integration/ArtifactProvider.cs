using System.Threading;

namespace Relativity.IntegrationPoints.Tests.Integration
{
    public static class ArtifactProvider
    {
        private static int _currentArtifactId = 100000;

        public static int NextId() => Interlocked.Increment(ref _currentArtifactId);
    }
}
