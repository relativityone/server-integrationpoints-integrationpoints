using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal interface IImportStreamBuilder
	{
		Stream Create(ISourceServiceFactoryForUser serviceFactory, Func<IObjectManager, Task<Stream>> streamFactory, StreamEncoding encoding);
	}
}
