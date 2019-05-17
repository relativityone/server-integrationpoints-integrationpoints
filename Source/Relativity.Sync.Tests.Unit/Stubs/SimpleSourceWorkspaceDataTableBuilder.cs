using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	/// <summary>
	///     Wraps the given batch in a DataTable - the column names
	///     are random strings and each row is the Values property of each object.
	/// </summary>
	internal sealed class SimpleSourceWorkspaceDataTableBuilder : ISourceWorkspaceDataTableBuilder
	{
		public Task<DataTable> BuildAsync(int workspaceArtifactId, RelativityObjectSlim[] batch)
		{
			DataTable dt = new DataTable();
			DataColumn[] columns = batch.First().Values.Select(_ => new DataColumn(Guid.NewGuid().ToString())).ToArray();
			dt.Columns.AddRange(columns);
			foreach (RelativityObjectSlim obj in batch)
			{
				dt.Rows.Add(obj.Values.ToArray());
			}

			return Task.FromResult(dt);
		}
	}
}
