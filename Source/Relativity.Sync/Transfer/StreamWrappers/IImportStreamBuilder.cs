using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.Services.Objects;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal interface IImportStreamBuilder
	{
		Stream Create(IRetriableStreamBuilder streamBuilder, StreamEncoding encoding);
	}
}
