using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Creates a single <see cref="IDataReader"/> for images, out of several sources of information based on the given schema.
	/// </summary>
	internal sealed class ImageBatchDataReaderBuilder : BatchDataReaderBuilderBase
	{
		public ImageBatchDataReaderBuilder(IFieldManager fieldManager, IExportDataSanitizer exportDataSanitizer)
			: base(fieldManager, exportDataSanitizer)
		{
		}

		protected override Task<IReadOnlyList<FieldInfoDto>> GetAllFieldsAsync(CancellationToken token)
		{
			return _fieldManager.GetImageAllFieldsAsync(token);
		}
	}
}
