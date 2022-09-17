using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
    internal sealed class FileLocationManager : IFileLocationManager
    {
        private readonly List<FmsBatchInfo> _fmsBatchesStorage;
        private readonly IAPILog _logger;
        private readonly ISynchronizationConfiguration _configuration;

        public FileLocationManager(IAPILog logger, ISynchronizationConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _fmsBatchesStorage = new List<FmsBatchInfo>();
        }

        public void TranslateAndStoreFilePaths(IDictionary<int, INativeFile> nativeFiles)
        {
            try
            {
                IDictionary<int, NativeFilePathStructure> pathStructures = GetNativeFilesPathStructure(nativeFiles);

                var commonSourcePathGroups = pathStructures.GroupBy(x => x.Value.FullDirectoryPath);
                Guid correlationId = Guid.NewGuid();
                foreach (var filePathComponents in commonSourcePathGroups)
                {
                    FmsBatchInfo batchInfo = new FmsBatchInfo(
                        _configuration.DestinationWorkspaceArtifactId,
                        filePathComponents.ToDictionary(x => x.Key, x => x.Value),
                        filePathComponents.Key,
                        correlationId);
                    _fmsBatchesStorage.Add(batchInfo);

                    SetNewPathForIapiTransfer(batchInfo, nativeFiles);
                    _logger.LogInformation("Created FMS batch for folder {fmsBatchFolderName}", batchInfo.SourceLocationShortPath);
                }

                _logger.LogInformation("Natives Files grouped by folder paths. Separated {pathStructuresCount}", commonSourcePathGroups.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not translate and store paths for FMS batches");
            }
        }

        public List<FmsBatchInfo> GetStoredLocations()
        {
            return _fmsBatchesStorage;
        }

        public void ClearStoredLocations()
        {
            if (_fmsBatchesStorage != null)
            {
                _fmsBatchesStorage.Clear();
            }
        }

        private IDictionary<int, NativeFilePathStructure> GetNativeFilesPathStructure(IDictionary<int, INativeFile> nativeFiles)
        {
            IDictionary<int, NativeFilePathStructure> structures = new Dictionary<int, NativeFilePathStructure>();
            foreach (KeyValuePair<int, INativeFile> file in nativeFiles)
            {
                try
                {
                    if (!string.IsNullOrEmpty(file.Value.Location))
                    {
                        structures.Add(file.Key, new NativeFilePathStructure(file.Value.Location));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not get correct structure of file path {filepath}", file.Value.Location);
                }
            }

            return structures;
        }

        private void SetNewPathForIapiTransfer(FmsBatchInfo batchInfo, IDictionary<int, INativeFile> nativeFiles)
        {
            foreach (FmsDocument document in batchInfo.Files)
            {
                INativeFile oldConfiguration = nativeFiles[document.DocumentArtifactId];
                nativeFiles[document.DocumentArtifactId] = new NativeFile(oldConfiguration.DocumentArtifactId, document.LinkForIAPI, oldConfiguration.Filename, oldConfiguration.Size);
            }
        }
    }
}
