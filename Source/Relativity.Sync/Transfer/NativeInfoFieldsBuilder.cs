using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Transfer
{
    internal sealed class NativeInfoFieldsBuilder : INativeInfoFieldsBuilder
    {
        private readonly INativeFileRepository _nativeFileRepository;
        private readonly IAntiMalwareHandler _antiMalwareHandler;
        private readonly IFileLocationManager _fileLocationManager;
        private readonly ISyncToggles _syncToggles;
        private readonly IAPILog _logger;

        public NativeInfoFieldsBuilder(
            INativeFileRepository nativeFileRepository,
            IAntiMalwareHandler antiMalwareHandler,
            IAPILog logger,
            IFileLocationManager fileLocationManager,
            ISyncToggles syncToggles)
        {
            _nativeFileRepository = nativeFileRepository;
            _antiMalwareHandler = antiMalwareHandler;
            _fileLocationManager = fileLocationManager;
            _syncToggles = syncToggles;
            _logger = logger;
        }

        public IEnumerable<FieldInfoDto> BuildColumns()
        {
            yield return FieldInfoDto.NativeFileFilenameField();
            yield return FieldInfoDto.NativeFileSizeField();
            yield return FieldInfoDto.NativeFileLocationField();
            yield return FieldInfoDto.SupportedByViewerField();
            yield return FieldInfoDto.RelativityNativeTypeField();
        }

        public async Task<INativeSpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, int[] documentArtifactIds)
        {
            IEnumerable<INativeFile> nativeFileInfo = await _nativeFileRepository
                .QueryAsync(sourceWorkspaceArtifactId, documentArtifactIds)
                .ConfigureAwait(false);

            IDictionary<int, INativeFile> artifactIdToNativeFile = new Dictionary<int, INativeFile>(documentArtifactIds.Length);

            foreach (INativeFile nativeFile in nativeFileInfo)
            {
                if (artifactIdToNativeFile.ContainsKey(nativeFile.DocumentArtifactId))
                {
                    artifactIdToNativeFile[nativeFile.DocumentArtifactId].IsDuplicated = true;
                    _logger.LogWarning("Duplicated native file detected for document Artifact ID: {artifactId}", nativeFile.DocumentArtifactId);
                }
                else
                {
                    artifactIdToNativeFile.Add(nativeFile.DocumentArtifactId, nativeFile);
                }
            }

            if (_syncToggles.IsEnabled<UseFMS>())
            {
                _fileLocationManager.TranslateAndStoreFilePaths(artifactIdToNativeFile);
            }

            return new NativeInfoRowValuesBuilder(artifactIdToNativeFile, _antiMalwareHandler);
        }
    }
}
