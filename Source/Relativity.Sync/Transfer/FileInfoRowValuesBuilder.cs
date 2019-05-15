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

		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new[] {SpecialFieldType.NativeFileFilename, SpecialFieldType.NativeFileFilename, SpecialFieldType.NativeFileFilename};

		public IEnumerable<object> BuildRowValues(FieldInfo fieldInfo, RelativityObjectSlim document, object initialValue)
		{
			switch (fieldInfo.SpecialFieldType)
			{
				case SpecialFieldType.NativeFileSize:
					yield return _artifactIdToNativeFile[document.ArtifactID].Size;
					break;
				case SpecialFieldType.NativeFileLocation:
					yield return _artifactIdToNativeFile[document.ArtifactID].Location;
					break;
				case SpecialFieldType.NativeFileFilename:
					yield return _artifactIdToNativeFile[document.ArtifactID].Filename;
					break;
			}
		}
	}
}