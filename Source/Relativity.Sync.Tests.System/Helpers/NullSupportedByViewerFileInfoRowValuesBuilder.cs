using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal class NullSupportedByViewerFileInfoRowValuesBuilder : INativeSpecialFieldRowValuesBuilder
	{
		private readonly INativeSpecialFieldRowValuesBuilder _fileInfoRowValuesBuilder;

		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => _fileInfoRowValuesBuilder.AllowedSpecialFieldTypes;

		public NullSupportedByViewerFileInfoRowValuesBuilder(INativeSpecialFieldRowValuesBuilder fileInfoRowValuesBuilder)
		{
			_fileInfoRowValuesBuilder = fileInfoRowValuesBuilder;
		}

		public object BuildRowValue(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, object initialValue)
		{
			if (fieldInfoDto.SpecialFieldType == SpecialFieldType.SupportedByViewer)
			{
				return null;
			}

			return _fileInfoRowValuesBuilder.BuildRowValue(fieldInfoDto, document, initialValue);
		}
	}
}
