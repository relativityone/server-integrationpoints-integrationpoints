using System;
using System.IO;
using System.Runtime.CompilerServices;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Data.Repositories;
using Polly;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	/// <summary>
	///     ImportAPI does not provide any way to dispose streams we're passing
	///     this stream will dispose itself when read
	///     it's working under assumption that IAPI won't access this stream twice
	/// </summary>
	internal sealed class SelfDisposingStream : Stream
	{
		private Stream _stream;

		private const int _MAX_RETRY_ATTEMPTS = 3;
		private const int _WAIT_INTERVAL_IN_SECONDS = 1;

		private readonly int _artifactID;
		private readonly int _fieldArtifactID;

		private readonly IAPILog _logger;
		private readonly IRelativityObjectManager _relativityObjectManager;
		private readonly IQueryFieldLookupRepository _fieldLookupRepository;
		private readonly Policy<Stream> _getStreamRetryPolicy;


		public SelfDisposingStream(
			int artifactID,
			int fieldArtifactID,
			IRelativityObjectManager relativityObjectManager,
			IQueryFieldLookupRepository fieldLookupRepository,
			IAPILog logger)
		{
			_relativityObjectManager = relativityObjectManager;
			_fieldLookupRepository = fieldLookupRepository;
			_artifactID = artifactID;
			_fieldArtifactID = fieldArtifactID;
			_getStreamRetryPolicy = CreateStreamRetryPolicy();

			_stream = _getStreamRetryPolicy.Execute(() => GetKeplerStream(_artifactID, _fieldArtifactID));

			_logger = logger;
		}

		public override void Flush()
		{
			LogUnsupportedCall();
			_stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			LogUnsupportedCall();
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			LogUnsupportedCall();
			_stream.SetLength(value);
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				DisposeInnerStream();
			}

		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				int readBytesCount = _stream.Read(buffer, offset, count);
				if (readBytesCount == 0)
				{
					DisposeInnerStream();
				}

				return readBytesCount;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed reading {0} bytes from {1} at index: {2}", count, nameof(SelfDisposingStream), offset);
				throw;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			LogUnsupportedCall();
			_stream.Write(buffer, offset, count);
		}

		public override bool CanRead
		{
			get
			{
				if (!_stream.CanRead)
				{
					_stream = _getStreamRetryPolicy.Execute(() => GetKeplerStream(_artifactID, _fieldArtifactID));
				}

				return _stream.CanRead;
			}
		}

		public override bool CanSeek => _stream.CanSeek;

		public override bool CanWrite => _stream.CanWrite;

		public override long Length => _stream.Length;

		public override long Position
		{
			get { return _stream.Position; }
			set { _stream.Position = value; }
		}

		private Stream GetKeplerStream(int artifactID, int fieldArtifactID)
		{
			try
			{
				var fieldRef = new FieldRef { ArtifactID = _fieldArtifactID };
				Stream stream = _relativityObjectManager.StreamLongTextAsync(artifactID, fieldRef)
					.GetAwaiter()
					.GetResult();

				ViewFieldInfo field = _fieldLookupRepository.GetFieldByArtifactId(fieldArtifactID);

				return field.IsUnicodeEnabled
					? stream
					: new AsciiToUnicodeStream(stream);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unable to create KeplerStream inside {0}", nameof(SelfDisposingStream));
				throw;
			}
		}

		private Policy<Stream> CreateStreamRetryPolicy()
		{
			return Policy
				.HandleResult<Stream>(s => s == null || !s.CanRead)
				.Or<Exception>()
				.WaitAndRetry(_MAX_RETRY_ATTEMPTS, i => TimeSpan.FromSeconds(_WAIT_INTERVAL_IN_SECONDS), onRetry: (outcome, timespan, retryAttempt, context) =>
				{
					DisposeInnerStream();
					_logger.LogWarning("Retrying Kepler Stream creation inside {0}. Attempt {1} of {2}", nameof(SelfDisposingStream), retryAttempt, _MAX_RETRY_ATTEMPTS);
				});
		}

		private void DisposeInnerStream()
		{
			_stream?.Dispose();
			_stream = null;
		}

		private void LogUnsupportedCall([CallerMemberName] string callerMemberName = "")
		{
			_logger.LogWarning("Unsupported operation on {className} ({functionName})", nameof(SelfDisposingStream), callerMemberName);
		}
	}
}