using System;
using System.Collections.Generic;
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
		private readonly FieldInfoDto _identifierField;

		public Action<string, string> ItemLevelErrorHandler { get; set; }

		public SimpleBatchDataReaderBuilder(FieldInfoDto identifierField)
		{
			_identifierField = identifierField;
		}

		public async Task<IBatchDataReader> BuildAsync(int workspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token)
		{
			await Task.Yield();
			DataTable dt = new DataTable();
			List<DataColumn> columns = new List<DataColumn>();
			for (int i = 0; i < batch.First().Values.Count; i++)
			{
				if (i == _identifierField.DocumentFieldIndex)
				{
					columns.Add(new DataColumn(_identifierField.SourceFieldName, typeof(object)));
				}
				else
				{
					columns.Add(new DataColumn(Guid.NewGuid().ToString(), typeof(object)));
				}
			}
			dt.Columns.AddRange(columns.ToArray());
			foreach (RelativityObjectSlim obj in batch)
			{
				dt.Rows.Add(obj.Values.ToArray());
			}
			DataTableReader dataReader = dt.CreateDataReader();
			return new SimpleBatchDataReader(dataReader);
		}
	}
}
