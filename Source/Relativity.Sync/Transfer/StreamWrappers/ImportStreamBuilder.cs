using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.Services.Objects;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	/// <summary>
	/// Wraps a <see cref="Stream"/> in several intermediate streams so it can be consistently
	/// used by the Relativity Import API. This includes regenerating the stream on error (due to
	/// transient API failures), translating ASCII -> Unicode, and self-disposal (since the Import
	/// API will not explicitly it).
	/// </summary>
	internal sealed class ImportStreamBuilder : IImportStreamBuilder
	{
		private readonly IStreamRetryPolicyFactory _streamRetryPolicyFactory;
		private readonly ISyncLog _logger;

		public ImportStreamBuilder(IStreamRetryPolicyFactory streamRetryPolicyFactory, ISyncLog logger)
		{
			_streamRetryPolicyFactory = streamRetryPolicyFactory;
			_logger = logger;
		}

		public Stream Create(Func<Task<IObjectManager>> objectManagerFactory, Func<IObjectManager, Task<Stream>> streamFactory, StreamEncoding encoding)
		{
			IObjectManager objectManager = objectManagerFactory().GetAwaiter().GetResult();
			Stream wrappedStream = new SelfRecreatingStream(objectManager, streamFactory, _streamRetryPolicyFactory, _logger);
			if (encoding == StreamEncoding.ASCII)
			{
				wrappedStream = new AsciiToUnicodeStream(wrappedStream);
			}
			var selfDisposingStream = new SelfDisposingStream(wrappedStream, _logger);
			return selfDisposingStream;
		}
	}
}
