using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerator
{
	public interface IFileGenerator
	{
		Task<FileInfo> GenerateAsync(string name, long sizeInBytes);
	}
}