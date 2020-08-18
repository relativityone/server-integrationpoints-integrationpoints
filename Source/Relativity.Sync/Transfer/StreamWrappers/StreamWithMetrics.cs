using System;
using System.IO;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal sealed class StreamWithMetrics : Stream
	{
		private readonly Stream _wrappedStream;
		private readonly IStopwatch _readTimeStopwatch;
		private readonly int _relativityObjectArtifactId;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly ISyncLog _logger;

		private bool _disposed;
		private int _readInvocationCount = 0;
		private long _totalBytesRead = 0;
		
		internal StreamWithMetrics(Stream wrappedStream, IStopwatch readTimeStopwatch, int relativityObjectArtifactId, IJobStatisticsContainer jobStatisticsContainer, ISyncLog logger)
		{
			_wrappedStream = wrappedStream;
			_readTimeStopwatch = readTimeStopwatch;
			_relativityObjectArtifactId = relativityObjectArtifactId;
			_jobStatisticsContainer = jobStatisticsContainer;
			_logger = logger;
		}

		public override void Flush()
		{
			_wrappedStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _wrappedStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_wrappedStream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			_readInvocationCount++;

			_readTimeStopwatch.Start();
			int read = _wrappedStream.Read(buffer, offset, count);
			_readTimeStopwatch.Stop();

			_totalBytesRead += read;

			return read;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_wrappedStream.Write(buffer, offset, count);
		}

		public override bool CanRead => _wrappedStream.CanRead;

		public override bool CanSeek => _wrappedStream.CanSeek;

		public override bool CanWrite => _wrappedStream.CanWrite;

		public override long Length => _wrappedStream.Length;

		public override long Position
		{
			get => _wrappedStream.Position;
			set => _wrappedStream.Position = value;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_logger.LogInformation("Disposing long text stream. Relativity Object ArtifactID: {artifactID} Total bytes read: {totalBytesRead} Total read time (sec): {totalReadTime} Read invocations count: {readCount}",
					_relativityObjectArtifactId, _totalBytesRead, Math.Round(_readTimeStopwatch.Elapsed.TotalSeconds, 3), _readInvocationCount);
				_jobStatisticsContainer.AppendLongTextStreamStatistics(new LongTextStreamStatistics()
				{
					TotalBytesRead = _totalBytesRead,
					TotalReadTime = _readTimeStopwatch.Elapsed
				});
				_disposed = true;
				_wrappedStream.Dispose();
			}
		}
	}
}