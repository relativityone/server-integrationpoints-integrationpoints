using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
    internal sealed class FolderPathFieldBuilder : INativeSpecialFieldBuilder
    {
        private readonly IFolderPathRetriever _folderPathRetriever;
        private readonly IFieldConfiguration _fieldConfiguration;

        public FolderPathFieldBuilder(IFolderPathRetriever folderPathRetriever, IFieldConfiguration fieldConfiguration)
        {
            _folderPathRetriever = folderPathRetriever;
            _fieldConfiguration = fieldConfiguration;
        }

        public IEnumerable<FieldInfoDto> BuildColumns()
        {
            if (_fieldConfiguration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
            {
                string folderPathFieldName = _fieldConfiguration.GetFolderPathSourceFieldName();
                yield return FieldInfoDto.FolderPathFieldFromDocumentField(folderPathFieldName);
            }
            else if (_fieldConfiguration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
            {
                yield return FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure();
            }
        }

        public async Task<INativeSpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds)
        {
            IDictionary<int, string> folderPathsMap = await BuildFolderPathsMapAsync(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false);
            return new FolderPathRowValueBuilder(_fieldConfiguration.DestinationFolderStructureBehavior, folderPathsMap);
        }

        private async Task<IDictionary<int, string>> BuildFolderPathsMapAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
        {
            IDictionary<int, string> folderPathsMap = null;
            if (_fieldConfiguration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
            {
                folderPathsMap = await _folderPathRetriever.GetFolderPathsAsync(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false);
            }
            return folderPathsMap;
        }
    }
}
