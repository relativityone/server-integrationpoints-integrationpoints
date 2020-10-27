using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Creates a single <see cref="IDataReader"/> for natives, out of several sources of information based on the given schema.
	/// </summary>
	internal sealed class NativeBatchDataReaderBuilder : BatchDataReaderBuilderBase
	{
		public NativeBatchDataReaderBuilder(IFieldManager fieldManager, IExportDataSanitizer exportDataSanitizer)
			: base(fieldManager, exportDataSanitizer)
		{
		}

		protected override Task<IReadOnlyList<FieldInfoDto>> GetAllFieldsAsync(CancellationToken token)
		{
			return _fieldManager.GetNativeAllFieldsAsync(token);
		}

		protected override Task<IDataReader> CreateDataReaderAsync(DataTable templateDataTable, int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch,
			CancellationToken token)
		{
			return Task.FromResult((IDataReader)new NativeBatchDataReader(templateDataTable, sourceWorkspaceArtifactId, batch, _allFields, _fieldManager, _exportDataSanitizer, ItemLevelErrorHandler, token));
		}
	}
}
