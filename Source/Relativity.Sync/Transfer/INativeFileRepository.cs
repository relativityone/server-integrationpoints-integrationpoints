using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal interface INativeFileRepository
	{
		Task<IEnumerable<INativeFile>> QueryAsync(int workspaceId, ICollection<int> documentIds);
		Task<long> CalculateNativesTotalSizeAsync(int workspaceId, QueryRequest request);
	}
}
