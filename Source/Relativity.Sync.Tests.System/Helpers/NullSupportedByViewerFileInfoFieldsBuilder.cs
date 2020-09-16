﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal class NullSupportedByViewerFileInfoFieldsBuilder : INativeSpecialFieldBuilder
	{
		private readonly NativeInfoFieldsBuilder _fileInfoFieldsBuilder;

		public NullSupportedByViewerFileInfoFieldsBuilder(INativeFileRepository nativeFileRepository)
		{
			_fileInfoFieldsBuilder = new NativeInfoFieldsBuilder(nativeFileRepository, new EmptyLogger());
		}

		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			return _fileInfoFieldsBuilder.BuildColumns();
		}

		public async Task<INativeSpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			return new NullSupportedByViewerFileInfoRowValuesBuilder(
				await _fileInfoFieldsBuilder.GetRowValuesBuilderAsync(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false)
			);
		}
	}
}
