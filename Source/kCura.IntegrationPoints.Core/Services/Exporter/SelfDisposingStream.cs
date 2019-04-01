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
		private int maxAttempts = 3;

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
				.WaitAndRetry(maxAttempts, i => TimeSpan.FromSeconds(1), onRetry: (outcome, timespan, retryAttempt, context) =>
				{
					_stream?.Dispose();
					_logger.LogWarning("Retrying Kepler Stream creation inside SelfDisposingStream. Attempt {0} of {1}", retryAttempt, maxAttempts);
				});

			_stream = retryPolicy.Execute(() => GetKeplerStream(_artifactID, _fieldArtifactID));

			_logger = logger;
		}

		private Stream GetKeplerStream(int artifactID, int fieldArtifactID)
		{
			try
			{
				_fieldRef = new FieldRef {ArtifactID = _fieldArtifactID};
				Stream stream = _relativityObjectManager.StreamLongTextAsync(artifactID, _fieldRef)
					.GetAwaiter()
					.GetResult();

				ViewFieldInfo field = _fieldLookupRepository.GetFieldByArtifactId(fieldArtifactID);
				if (!field.IsUnicodeEnabled)
				{
					stream = new AsciiToUnicodeStream(stream);
				}

				return stream;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unable to create KeplerStream inside SelfDisposingStream");
				throw;
			}
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

		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				int readBytes = _stream.Read(buffer, offset, count);
				if (readBytes == 0)
				{
					_stream.Dispose();
				}

				return readBytes;
			}catch (System.Exception ex)
			{
				_logger.LogError(ex, "Failed reading {0} bytes from SelfDisposingStream at index: {1}", count, offset);
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
	}
}