using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal sealed class NativeInfoRowValuesBuilder : INativeSpecialFieldRowValuesBuilder
    {
        public IDictionary<int, INativeFile> ArtifactIdToNativeFile { get; set; }

        public NativeInfoRowValuesBuilder(IDictionary<int, INativeFile> artifactIdToNativeFile)
        {
            ArtifactIdToNativeFile = artifactIdToNativeFile;
        }

        public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new[]
        {
            SpecialFieldType.NativeFileFilename,
            SpecialFieldType.NativeFileLocation,
            SpecialFieldType.NativeFileSize,
            SpecialFieldType.SupportedByViewer,
            SpecialFieldType.RelativityNativeType
        };

        public object BuildRowValue(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, object initialValue)
        {
            if (fieldInfoDto.IsDocumentField)
            {
                // This will apply to "special" file fields that are also normal fields on Document, e.g. native file type
                return initialValue;
            }

            if (!ArtifactIdToNativeFile.TryGetValue(document.ArtifactID, out INativeFile nativeFile))
            {
                nativeFile = NativeFile.Empty;
            }

            if (nativeFile.IsDuplicated)
            {
                throw new SyncItemLevelErrorException($"Database is corrupted - document Artifact ID: {document.ArtifactID} has more than one native file associated with it.");
            }

            switch (fieldInfoDto.SpecialFieldType)
            {
                case SpecialFieldType.NativeFileSize:
                    return nativeFile.Size;
                case SpecialFieldType.NativeFileLocation:
                    return nativeFile.Location;
                case SpecialFieldType.NativeFileFilename:
                    return nativeFile.Filename;
                default:
                    throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfoDto.SpecialFieldType.ToString()}.", nameof(fieldInfoDto));
            }
        }
    }
}