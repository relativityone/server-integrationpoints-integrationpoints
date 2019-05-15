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

		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new[] {SpecialFieldType.NativeFileFilename, SpecialFieldType.NativeFileLocation, SpecialFieldType.NativeFileSize};

		public object BuildRowValue(FieldInfo fieldInfo, RelativityObjectSlim document, object initialValue)
		{
			switch (fieldInfo.SpecialFieldType)
			{
				case SpecialFieldType.NativeFileSize:
					return _artifactIdToNativeFile[document.ArtifactID].Size;
				case SpecialFieldType.NativeFileLocation:
					return _artifactIdToNativeFile[document.ArtifactID].Location;
				case SpecialFieldType.NativeFileFilename:
					return _artifactIdToNativeFile[document.ArtifactID].Filename;
				default:
					throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfo.SpecialFieldType.ToString()}.", nameof(fieldInfo));
			}
		}
	}
}