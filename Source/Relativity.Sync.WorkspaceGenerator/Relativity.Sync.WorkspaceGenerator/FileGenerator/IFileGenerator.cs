using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerator
{
	public interface IFileGenerator
	{
		Task<IEnumerable<FileInfo>> GenerateAsync(int count, long totalSizeInMB);
	}
}