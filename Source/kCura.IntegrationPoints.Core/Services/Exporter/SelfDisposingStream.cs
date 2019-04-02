using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Polly;
using Relativity;
using Relativity.API;
using Relativity.Logging;
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
		private IAPILog _logger;
		private IRelativityObjectManager _relativityObjectManager;
		private IQueryFieldLookupRepository _fieldLookupRepository;
		private int _artifactID;
		private FieldRef _fieldRef;
		private int _fieldArtifactID;
		private Policy<Stream> retryPolicy;
		private const int MAX_RETRY_ATTEMPTS = 3;
		private const int WAIT_INTERVAL_IN_SECONDS = 1;

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
			retryPolicy = Policy
				.HandleResult<Stream>(s => s == null || !s.CanRead)
				.Or<Exception>()
				.WaitAndRetry(MAX_RETRY_ATTEMPTS, i => TimeSpan.FromSeconds(WAIT_INTERVAL_IN_SECONDS), onRetry: (outcome, timespan, retryAttempt, context) =>
				{
					DisposeInnerStream();
					_logger.LogWarning("Retrying Kepler Stream creation inside {0}. Attempt {1} of {2}", nameof(SelfDisposingStream), retryAttempt, MAX_RETRY_ATTEMPTS);
				});

			_stream = retryPolicy.Execute(() => GetKeplerStream(_artifactID, _fieldArtifactID));

			_logger = logger;
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
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
				int readBytes = _stream.Read(buffer, offset, count);
				if (readBytes == 0)
				{
					DisposeInnerStream();
				}

				return readBytes;
			}
			catch (System.Exception ex)
			{
				_logger.LogError(ex, "Failed reading {0} bytes from {1} at index: {2}", count, nameof(SelfDisposingStream), offset);
				throw;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}

		public override bool CanRead
		{
			get
			{
				if (!_stream.CanRead)
				{
					_stream = retryPolicy.Execute(() => GetKeplerStream(_artifactID, _fieldArtifactID));
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
				_fieldRef = new FieldRef { ArtifactID = _fieldArtifactID };
				Stream stream = _relativityObjectManager.StreamLongTextAsync(artifactID, _fieldRef)
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

		private void DisposeInnerStream()
		{
			_stream.Dispose();
		}
	}
}