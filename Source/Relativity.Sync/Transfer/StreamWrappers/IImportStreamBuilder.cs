using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.Services.Objects;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal interface IImportStreamBuilder
	{
		Stream Create(Func<Task<IObjectManager>> objectManagerFactory, Func<IObjectManager,Task<Stream>> streamFactory, StreamEncoding encoding);
	}
}
