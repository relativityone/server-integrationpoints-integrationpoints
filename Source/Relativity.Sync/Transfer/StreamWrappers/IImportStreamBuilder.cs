using System;
using System.IO;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal interface IImportStreamBuilder
	{
		Stream Create(Func<Stream> streamFunc, StreamEncoding encoding);
	}
}
