using System;
using System.Diagnostics;

namespace kCura.IntegrationPoints.Core.Telemetry
{
    public class Instrumentation : IDisposable
    {
        public const string OTEL_ACTIVITY_SOURCE_NAME = "Relativity.IntegrationPoints";

        private bool _disposed;

        public Instrumentation()
        {
            this.ActivitySource = new ActivitySource(OTEL_ACTIVITY_SOURCE_NAME);
        }

        public ActivitySource ActivitySource { get; private set; }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.ActivitySource?.Dispose();
                    this.ActivitySource = null;
                }

                _disposed = true;
            }
        }
    }
}