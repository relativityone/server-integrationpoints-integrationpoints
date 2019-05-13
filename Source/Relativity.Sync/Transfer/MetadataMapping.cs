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
			DestinationFolderStructureBehavior = destinationFolderStructureBehavior;
			_folderPathSourceFieldArtifactId = folderPathSourceFieldArtifactId;
			FieldMappings = fieldMappings.AsReadOnly();

			FolderPathFieldName = "FolderPath";
			NativeFileLocationFieldName = "NativeFileLocation";
			NativeFileSizeFieldName = "NativeFileSize";
			NativeFileFilenameFieldName = "NativeFileFilename";
		}

		public IReadOnlyList<FieldMap> FieldMappings { get; }
		public string FolderPathFieldName { get; }
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

		/// <summary>
		/// Columns for the DataTable to be passed to IAPI. This includes "real" fields (i.e. those mapping to an actual Field in Relativity)
		/// and derived fields (e.g. native information, folder path (in some cases), etc.).
		/// </summary>
		public DataColumn[] CreateDataTableColumns()
		{
			IEnumerable<FieldEntry> documentFields = GetDocumentFields();
			IEnumerable<DataColumn> documentFieldColumns = documentFields.Select(x => new DataColumn(x.DisplayName));

			IEnumerable<DataColumn> specialFieldColumns = GetSpecialFieldDataColumns();

			return documentFieldColumns.Concat(specialFieldColumns).ToArray();
		}

		/// <summary>
		/// Creates an object array out of a document's literal field values plus its special values.
		/// </summary>
		/// <param name="documentFieldValues">Values of a document's fields returned from an earlier read</param>
		/// <param name="sourceWorkspaceFolderPath">Folder path of the document in its source workspace; will be used only if source folder path is being retained</param>
		/// <param name="nativeFileInfo">Metadata about this object's native file. If it does not have a native, use <see cref="NativeFile.Empty"/>.</param>
		/// <returns></returns>
		public object[] CreateDataTableRow(IEnumerable<object> documentFieldValues, string sourceWorkspaceFolderPath, INativeFile nativeFileInfo)
		{
			IEnumerable<object> specialFieldValues = GetSpecialFieldDataValues(sourceWorkspaceFolderPath, nativeFileInfo);

			return documentFieldValues.Concat(specialFieldValues).ToArray();
		}
		
		private static FieldRef FieldEntryToFieldRef(FieldEntry field)
		{
			return field.FieldIdentifier > 0
				? new FieldRef { ArtifactID = field.FieldIdentifier }
				: new FieldRef { Name = field.DisplayName };
		}

		private IEnumerable<DataColumn> GetSpecialFieldDataColumns()
		{
			if (DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			{
				yield return new DataColumn(FolderPathFieldName, typeof(string));
			}

			yield return new DataColumn(NativeFileLocationFieldName, typeof(string));
			yield return new DataColumn(NativeFileSizeFieldName, typeof(long));
			yield return new DataColumn(NativeFileFilenameFieldName, typeof(string));
		}

		private IEnumerable<FieldEntry> GetDocumentFields()
		{
			foreach (FieldMap fieldMap in FieldMappings)
			{
				yield return fieldMap.SourceField;
			}

			yield return new FieldEntry { DisplayName = _SUPPORTED_BY_VIEWER_FIELD_NAME, IsIdentifier = false };
			yield return new FieldEntry { DisplayName = _RELATIVITY_NATIVE_TYPE_FIELD_NAME, IsIdentifier = false };

			if (DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				yield return new FieldEntry { FieldIdentifier = _folderPathSourceFieldArtifactId };
			}
		}
		
		private IEnumerable<object> GetSpecialFieldDataValues(string folderPath, INativeFile nativeFile)
		{
			if (DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			{
				yield return folderPath;
			}

			yield return nativeFile.Filename;
			yield return nativeFile.Size;
			yield return nativeFile.Location;
		}
	}
}
