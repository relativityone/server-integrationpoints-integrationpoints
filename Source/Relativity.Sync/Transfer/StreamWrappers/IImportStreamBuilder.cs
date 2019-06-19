using System;
using System.IO;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal interface IImportStreamBuilder
	{
		Stream Create(Func<Task<Stream>> streamFunc, StreamEncoding encoding);
	}
}
