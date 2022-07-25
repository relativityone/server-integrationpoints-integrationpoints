using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System.Helpers
{
    internal class NullSupportedByViewerFileInfoFieldsBuilder : INativeSpecialFieldBuilder
    {
        private readonly NativeInfoFieldsBuilder _fileInfoFieldsBuilder;

        public NullSupportedByViewerFileInfoFieldsBuilder(INativeFileRepository nativeFileRepository, IFileLocationManager fileLocationManager)
        {
            _fileInfoFieldsBuilder = new NativeInfoFieldsBuilder(nativeFileRepository, null, new EmptyLogger(), fileLocationManager);
        }

        public IEnumerable<FieldInfoDto> BuildColumns()
        {
            return _fileInfoFieldsBuilder.BuildColumns();
        }

        public async Task<INativeSpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds)
        {
            return new NullSupportedByViewerFileInfoRowValuesBuilder(
                await _fileInfoFieldsBuilder.GetRowValuesBuilderAsync(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false)
            );
        }
    }
}
