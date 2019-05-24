using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	/// <summary>
	///     Wraps the given batch in a DataTable - the column names
	///     are random strings and each row is the Values property of each object.
	/// </summary>
	internal sealed class SimpleBatchDataReaderBuilder : IBatchDataReaderBuilder
	{
		public async Task<IDataReader> BuildAsync(int workspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token)
		{
			await Task.Yield();
			DataTable dt = new DataTable();
			DataColumn[] columns = batch.First().Values.Select(_ => new DataColumn(Guid.NewGuid().ToString())).ToArray();
			dt.Columns.AddRange(columns);
			foreach (RelativityObjectSlim obj in batch)
			{
				dt.Rows.Add(obj.Values.ToArray());
			}
			DataTableReader dataReader = dt.CreateDataReader();
			return dataReader;
		}
	}
}
