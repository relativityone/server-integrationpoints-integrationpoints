﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Contains data for single FMS upload Job. Sync batch may contain many FMS batches.
    /// </summary>
    internal sealed class FmsBatchInfo
    {
        private readonly string _serverName;
        private readonly int _destinationWorkspaceArtifactId;

        public List<FmsDocument> Files { get; internal set; }

        public Guid TraceId { get; }

        public string SourceLocationShortPath { get; }

        public string DestinationLocationShortPath => $@"Files/EDDS{_destinationWorkspaceArtifactId}/RV_{TraceId}";

        public string UploadedBatchFilePath { get; set; }

        internal FmsBatchInfo(int destinationWorkspaceArtifactId, IDictionary<int, NativeFilePathStructure> filePaths, string sourceDirectoryPath, Guid correlationId)
        {
            _serverName = filePaths.Select(x => x.Value.ServerPath).First();
            _destinationWorkspaceArtifactId = destinationWorkspaceArtifactId;
            TraceId = correlationId;
            SourceLocationShortPath = CreateShortLocationSourcePath(sourceDirectoryPath);
            PrepareListOfFiles(filePaths);
        }

        private void PrepareListOfFiles(IDictionary<int, NativeFilePathStructure> files)
        {
            Files = new List<FmsDocument>();
            foreach (KeyValuePair<int, NativeFilePathStructure> file in files)
            {
                Files.Add(new FmsDocument(file.Key, file.Value.FileName, CreateLinkForIAPI(file.Value.FileName)));
            }
        }

        private string CreateLinkForIAPI(string fileName)
        {
            return $@"{_serverName}\{DestinationLocationShortPath.Replace("/", @"\")}\{fileName}";
        }

        private string CreateShortLocationSourcePath(string sourceDirectoryPath)
        {
            return sourceDirectoryPath
                .Replace($@"{_serverName}\", string.Empty)
                .Replace(@"\", "/");
        }
    }
}
