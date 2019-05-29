using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class FileInfoRowValuesBuilder : ISpecialFieldRowValuesBuilder
	{
		private readonly IDictionary<int, INativeFile> _artifactIdToNativeFile;

		public FileInfoRowValuesBuilder(IDictionary<int, INativeFile> artifactIdToNativeFile)
		{
			_artifactIdToNativeFile = artifactIdToNativeFile;
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

			if (!_artifactIdToNativeFile.ContainsKey(document.ArtifactID))
			{
				throw new SyncException("Mapping from document artifact id to native file was not found.");
			}

			switch (fieldInfoDto.SpecialFieldType)
			{
				case SpecialFieldType.NativeFileSize:
					return _artifactIdToNativeFile[document.ArtifactID].Size;
				case SpecialFieldType.NativeFileLocation:
					return _artifactIdToNativeFile[document.ArtifactID].Location;
				case SpecialFieldType.NativeFileFilename:
					return _artifactIdToNativeFile[document.ArtifactID].Filename;
				default:
					throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfoDto.SpecialFieldType.ToString()}.", nameof(fieldInfoDto));
			}
		}
	}
}