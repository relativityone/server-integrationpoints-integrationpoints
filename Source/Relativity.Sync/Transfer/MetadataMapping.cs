using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Represents the mapping between different fields and properties in the source and destination
	/// workspaces. This class should be the source of truth for what fields are mapped and how between
	/// the various Relativity APIs.
	/// </summary>
	internal sealed class MetadataMapping
	{
		private const string _SUPPORTED_BY_VIEWER_FIELD_NAME = "SupportedByViewer";
		private const string _RELATIVITY_NATIVE_TYPE_FIELD_NAME = "RelativityNativeType";

		private readonly int _folderPathSourceFieldArtifactId;

		public MetadataMapping(DestinationFolderStructureBehavior destinationFolderStructureBehavior,
			int folderPathSourceFieldArtifactId,
			List<FieldMap> fieldMappings)
		{
			_folderPathSourceFieldArtifactId = folderPathSourceFieldArtifactId;

			DestinationFolderStructureBehavior = destinationFolderStructureBehavior;
			FieldMappings = fieldMappings.AsReadOnly();

			FolderPathFieldName = "FolderPath";

			NativeFileLocationFieldName = "NativeFileLocation";
			NativeFileSizeFieldName = "NativeFileSize";
			NativeFileFilenameFieldName = "NativeFileFilename";

			SourceWorkspaceFieldName = "Relativity Source Case";
			SourceJobFieldName = "Relativity Source Job";
		}

		public IReadOnlyList<FieldMap> FieldMappings { get; }
		public string FolderPathFieldName { get; }
		public string SourceWorkspaceFieldName { get; }
		public string SourceJobFieldName { get; }
		public string NativeFileLocationFieldName { get; }
		public string NativeFileSizeFieldName { get; }
		public string NativeFileFilenameFieldName { get; }
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }

		/// <summary>
		/// Field refs mapping to actual fields on the object type.
		/// </summary>
		public IEnumerable<FieldRef> GetDocumentFieldRefs()
		{
			return GetDocumentFields().Select(FieldEntryToFieldRef);
		}

		private static FieldRef FieldEntryToFieldRef(FieldEntry field)
		{
			return field.FieldIdentifier > 0
				? new FieldRef { ArtifactID = field.FieldIdentifier }
				: new FieldRef { Name = field.DisplayName };
		}

		public IEnumerable<FieldEntry> GetSpecialFields()
		{
			//if (DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			//{
			yield return new FieldEntry { SpecialFieldType = SpecialFieldType.FolderPath, DisplayName = FolderPathFieldName, ValueType = typeof(string) };
			//}

			yield return new FieldEntry { SpecialFieldType = SpecialFieldType.NativeFileFilename, DisplayName = NativeFileFilenameFieldName, ValueType = typeof(string) };
			yield return new FieldEntry { SpecialFieldType = SpecialFieldType.NativeFileSize, DisplayName = NativeFileSizeFieldName, ValueType = typeof(long) };
			yield return new FieldEntry { SpecialFieldType = SpecialFieldType.NativeFileLocation, DisplayName = NativeFileLocationFieldName, ValueType = typeof(string) };
			yield return new FieldEntry { SpecialFieldType = SpecialFieldType.SourceWorkspace, DisplayName = SourceWorkspaceFieldName, ValueType = typeof(int) };
			yield return new FieldEntry { SpecialFieldType = SpecialFieldType.SourceJob, DisplayName = SourceJobFieldName, ValueType = typeof(int) };
		}

		public IEnumerable<FieldEntry> GetDocumentFields()
		{
			foreach (FieldMap fieldMap in FieldMappings)
			{
				yield return fieldMap.SourceField;
			}

			yield return new FieldEntry { DisplayName = _SUPPORTED_BY_VIEWER_FIELD_NAME, IsIdentifier = false };
			yield return new FieldEntry { DisplayName = _RELATIVITY_NATIVE_TYPE_FIELD_NAME, IsIdentifier = false };

			if (DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				yield return new FieldEntry { SpecialFieldType = SpecialFieldType.ReadFromFieldFolderPath, FieldIdentifier = _folderPathSourceFieldArtifactId };
			}
		}
	}
}
