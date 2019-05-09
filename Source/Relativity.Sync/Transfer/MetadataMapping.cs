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
		}

		public IReadOnlyList<FieldMap> FieldMappings { get; }
		public string FolderPathFieldName { get; }
		public string NativeFilePathFieldName { get; }
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }
		// + other derived field names (native file type, etc.)?

		/// <summary>
		/// Field refs mapping to actual fields on the object type.
		/// </summary>
		public IEnumerable<FieldRef> GetFieldRefs()
		{
			foreach (FieldMap fieldMap in FieldMappings)
			{
				yield return new FieldRef { ArtifactID = fieldMap.SourceField.FieldIdentifier };
			}

			yield return new FieldRef { Name = _SUPPORTED_BY_VIEWER_FIELD_NAME };
			yield return new FieldRef { Name = _RELATIVITY_NATIVE_TYPE_FIELD_NAME };

			if (DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				//_logger.LogVerbose("Including field {artifactId} used to retrieving destination folder structure.", _folderPathSourceFieldArtifactId);
				yield return new FieldRef { ArtifactID = _folderPathSourceFieldArtifactId };
			}
		}

		/// <summary>
		/// Columns for the DataTable to be passed to IAPI. This includes "real" fields (i.e. those mapping to an actual Field in Relativity)
		/// and derived fields (e.g. native information, folder path (in some cases), etc.).
		/// </summary>
		public DataColumn[] GetColumnsForDataTable()
		{
			IEnumerable<DataColumn> fieldMappings = FieldMappings.Select(x => new DataColumn(x.SourceField.DisplayName));
			IEnumerable<DataColumn> specialFieldMappings = GetSpecialFields();
			return fieldMappings.Concat(specialFieldMappings).ToArray();
		}

		private IEnumerable<DataColumn> GetSpecialFields()
		{
			yield return new DataColumn(_SUPPORTED_BY_VIEWER_FIELD_NAME); // ImportSettings.SupportedByViewerColumn
			yield return new DataColumn(_RELATIVITY_NATIVE_TYPE_FIELD_NAME); // 

			if (DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				//_logger.LogVerbose("Including field {artifactId} used to retrieving destination folder structure.", _folderPathSourceFieldArtifactId);
				yield return new DataColumn(FolderPathFieldName);
			}
		}
	}
}
