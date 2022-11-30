using System;
using System.IO;
using Relativity.API;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    /// <summary>
    /// Wraps a <see cref="Stream"/> in several intermediate streams so it can be consistently
    /// used by the Relativity Import API. This includes regenerating the stream on error (due to
    /// transient API failures), translating ASCII -> Unicode, self-disposal (since the Import
    /// API will not explicitly dispose it), and stream metrics.
    /// </summary>
    internal sealed class ImportStreamBuilder : IImportStreamBuilder
    {
        private readonly Func<IStopwatch> _stopwatchFactory;
        private readonly IJobStatisticsContainer _jobStatisticsContainer;
        private readonly IAPILog _logger;

        public ImportStreamBuilder(Func<IStopwatch> stopwatchFactory, IJobStatisticsContainer jobStatisticsContainer, IAPILog logger)
        {
            _stopwatchFactory = stopwatchFactory;
            _jobStatisticsContainer = jobStatisticsContainer;
            _logger = logger;
        }

        public Stream Create(IRetriableStreamBuilder streamBuilder, StreamEncoding encoding, int documentArtifactID)
        {
            Stream wrappedStream = new SelfRecreatingStream(streamBuilder, _logger);
            if (encoding == StreamEncoding.ASCII)
            {
                wrappedStream = new AsciiToUnicodeStream(wrappedStream);
            }

            var streamWithMetrics = new StreamWithMetrics(wrappedStream, _stopwatchFactory(), documentArtifactID, _jobStatisticsContainer, _logger);
            var selfDisposingStream = new SelfDisposingStream(streamWithMetrics, _logger);
            return selfDisposingStream;
        }
    }
}
