using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
	public interface IDataReaderProvider
	{
		IDataReader GetNextDataReader();
	}
}
