using Relativity.API;
using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Transfer
{
    internal sealed class FileLocationManager : IFileLocationManager
    {
        private IList<FmsBatchInfo> _fmsBatchesStorage;
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

                if (pathStructures.Any())
                {
                    HashSet<string> distinctSourcePaths = new HashSet<string>(pathStructures.Values.Select(x => x.FullDirectoryPath));

                    foreach (string distinctPath in distinctSourcePaths)
                    {
                        IDictionary<int, NativeFilePathStructure> filePathComponents = pathStructures.Where(x => x.Value.FullDirectoryPath == distinctPath).ToDictionary(d => d.Key, d => d.Value);
                        FmsBatchInfo batchInfo = new FmsBatchInfo(_configuration.DestinationWorkspaceArtifactId, filePathComponents, distinctPath);
                        _fmsBatchesStorage.Add(batchInfo);

                        SetNewPathForIapiTransfer(batchInfo, nativeFiles);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not translate and store paths for FMS batches");
            }
        }

        private void SetNewPathForIapiTransfer(FmsBatchInfo batchInfo, IDictionary<int, INativeFile> nativeFiles)
        {
            foreach (FmsDocument document in batchInfo.Files)
            {
                INativeFile oldConfiguration = nativeFiles[document.DocumentArtifactId];
                nativeFiles[document.DocumentArtifactId] = new NativeFile(oldConfiguration.DocumentArtifactId, document.LinkForIAPI, oldConfiguration.Filename, oldConfiguration.Size);
            }
        }

        public IList<FmsBatchInfo> GetStoredLocations()
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
    }
}
