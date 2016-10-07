using System;
using kCura.EDDS.WebAPI.ExportManagerBase;
using kCura.EDDS.WebAPI.FieldManagerBase;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using Relativity.Core;
using Relativity.MassImport;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public static class ExportConvertingExtensions
	{
		public static InitializationResults ToInitializationResults(this Export.InitializationResults result)
		{
			if (result == null)
			{
				return null;
			}
			return new InitializationResults
			{
				ColumnNames = result.ColumnNames,
				RowCount = result.RowCount,
				RunId = result.RunId
			};
		}

		public static ProductionInfo ToProductionInfo(this global::Relativity.Production.ProductionInfo info)
		{
			if (info == null)
			{
				return null;
			}
			return new ProductionInfo
			{
				Name = info.Name,
				BatesNumbering = info.BatesNumbering,
				BeginBatesReflectedFieldId = info.BeginBatesReflectedFieldId,
				DocumentsHaveRedactions = info.DocumentsHaveRedactions,
				IncludeImageLevelNumberingForDocumentLevelNumbering = info.IncludeImageLevelNumberingForDocumentLevelNumbering,
				UseDocumentLevelNumbering = info.UseDocumentLevelNumbering
			};
		}

		public static Field ToField(this global::Relativity.Core.DTO.Field field)
		{
			if (field == null)
			{
				return null;
			}
			ImportBehaviorChoice? importBehaviorChoice = null;
			if (field.ImportBehavior.HasValue)
			{
				importBehaviorChoice = (ImportBehaviorChoice)Enum.Parse(typeof(ImportBehaviorChoice), field.ImportBehavior.Value.ToString(), true);
			}

			return new Field
			{
				FieldArtifactTypeID = field.FieldArtifactTypeID,
				DisplayName = field.DisplayName,
				FieldTypeID = field.FieldTypeID,
				FieldCategoryID = field.FieldCategoryID,
				ArtifactViewFieldID = field.ArtifactViewFieldID,
				CodeTypeID = field.CodeTypeID,
				MaxLength = field.MaxLength,
				IsRequired = field.IsRequired,
				IsRemovable = field.IsRemovable,
				IsEditable = field.IsEditable,
				IsVisible = field.IsVisible,
				IsArtifactBaseField = field.IsArtifactBaseField,
				Value = field.Value,
				TableName = field.TableName,
				ColumnName = field.ColumnName,
				IsReadOnlyInLayout = field.IsReadOnlyInLayout,
				FilterType = field.FilterType,
				FieldDisplayTypeID = field.FieldDisplayTypeID,
				Rows = field.Rows,
				IsLinked = field.IsLinked,
				FormatString = field.FormatString,
				RepeatColumn = field.RepeatColumn,
				AssociativeArtifactTypeID = field.AssociativeArtifactTypeID,
				IsAvailableToAssociativeObjects = field.IsAvailableToAssociativeObjects,
				IsAvailableInChoiceTree = field.IsAvailableInChoiceTree,
				IsGroupByEnabled = field.IsGroupByEnabled,
				IsIndexEnabled = field.IsIndexEnabled,
				DisplayValueTrue = field.DisplayValueTrue,
				DisplayValueFalse = field.DisplayValueFalse,
				Width = field.Width,
				Wrapping = field.Wrapping,
				LinkLayoutArtifactID = field.LinkLayoutArtifactID,
				NameValue = field.NameValue,
				LinkType = field.LinkType,
				UseUnicodeEncoding = field.UseUnicodeEncoding,
				AllowHtml = field.AllowHtml,
				IsSortable = field.IsSortable,
				FriendlyName = field.FriendlyName,
				EnableDataGrid = field.EnableDataGrid,
				OverlayBehavior = field.OverlayBehavior,
				RelationalIndexViewArtifactID = field.RelationalIndexViewArtifactID,
				AllowGroupBy = field.AllowGroupBy,
				AllowPivot = field.AllowPivot,
				PopupPickerView = field.PopupPickerView,
				FieldTreeView = field.FieldTreeView,
				AvailableInViewer = field.AvailableInViewer,
				RelativityApplications = field.RelativityApplications,
				AutoAddChoices = field.AutoAddChoices,
				ArtifactID = field.ArtifactID,
				ArtifactTypeID = field.ArtifactTypeID,
				ParentArtifactID = field.ParentArtifactID,
				ContainerID = field.ContainerID,
				AccessControlListID = field.AccessControlListID,
				AccessControlListIsInherited = field.AccessControlListIsInherited,
				Keywords = field.Keywords,
				Notes = field.Notes,
				TextIdentifier = field.TextIdentifier,
				LastModifiedOn = field.LastModifiedOn,
				LastModifiedBy = field.LastModifiedBy,
				CreatedBy = field.CreatedBy,
				CreatedOn = field.CreatedOn,
				DeleteFlag = field.DeleteFlag,
				Guids = field.Guids.ToArray(),
				FieldType = (FieldType)Enum.Parse(typeof(FieldType), field.FieldType.ToString(), true),
				FieldCategory = (FieldCategory)Enum.Parse(typeof(FieldCategory), field.FieldCategory.ToString(), true),
				ImportBehavior = importBehaviorChoice,
				KeyboardShortcut = field.KeyboardShortcut.ToKeyboardShortcut(),
				ObjectsFieldArgs = field.ObjectsFieldArgs.ToObjectsFieldParameters(),
				RelationalPane = field.RelationalPane.ToRelationalFieldPane()
			};
		}

		private static RelationalFieldPane ToRelationalFieldPane(this global::Relativity.Core.DTO.RelationalFieldPane fieldPane)
		{
			if (fieldPane == null)
			{
				return null;
			}
			return new RelationalFieldPane
			{
				ColumnName = fieldPane.ColumnName,
				FieldArtifactID = fieldPane.FieldArtifactID,
				HeaderText = fieldPane.HeaderText,
				IconFileData = fieldPane.IconFileData,
				IconFilename = fieldPane.IconFilename,
				PaneID = fieldPane.PaneID,
				PaneOrder = fieldPane.PaneOrder,
				RelationalViewArtifactID = fieldPane.RelationalViewArtifactID
			};
		}

		private static ObjectsFieldParameters ToObjectsFieldParameters(this global::Relativity.Core.DTO.Field.ObjectsFieldParameters parameters)
		{
			if (parameters == null)
			{
				return null;
			}
			return new ObjectsFieldParameters
			{
				CreateForeignKeys = parameters.CreateForeignKeys,
				FieldSchemaColumnName = parameters.FieldSchemaColumnName,
				RelationalTableSchemaName = parameters.RelationalTableSchemaName,
				SiblingFieldName = parameters.SiblingFieldName,
				SiblingFieldSchemaColumnName = parameters.SiblingFieldSchemaColumnName
			};
		}

		private static KeyboardShortcut ToKeyboardShortcut(this global::Relativity.Core.DTO.KeyboardShortcut keyboardShortcut)
		{
			if (keyboardShortcut == null)
			{
				return null;
			}
			return new KeyboardShortcut
			{
				Id = keyboardShortcut.Id,
				Alt = keyboardShortcut.Alt,
				Ctrl = keyboardShortcut.Ctrl,
				Key = keyboardShortcut.Key,
				Shift = keyboardShortcut.Shift
			};
		}

		public static ExportStatistics ToExportStatistics(this EDDS.WebAPI.AuditManagerBase.ExportStatistics stats)
		{
			if (stats == null)
			{
				return null;
			}
			return new ExportStatistics
			{
				ExportImages = stats.ExportImages,
				Fields = stats.Fields,
				ArtifactTypeID = stats.ArtifactTypeID,
				AppendOriginalFilenames = stats.AppendOriginalFilenames,
				Bound = stats.Bound,
				CopyFilesFromRepository = stats.CopyFilesFromRepository,
				DataSourceArtifactID = stats.DataSourceArtifactID,
				Delimiter = stats.Delimiter,
				DestinationFilesystemFolder = stats.DestinationFilesystemFolder,
				DocumentExportCount = stats.DocumentExportCount,
				ErrorCount = stats.ErrorCount,
				ExportMultipleChoiceFieldsAsNested = stats.ExportMultipleChoiceFieldsAsNested,
				ExportNativeFiles = stats.ExportNativeFiles,
				ExportTextFieldAsFiles = stats.ExportTextFieldAsFiles,
				ExportedTextFieldID = stats.ExportedTextFieldID,
				ExportedTextFileEncodingCodePage = stats.ExportedTextFileEncodingCodePage,
				FileExportCount = stats.FileExportCount,
				FilePathSettings = stats.FilePathSettings,
				ImageFileType = (ImageFileExportType)Enum.Parse(typeof(ImageFileExportType), stats.ImageFileType.ToString(), true),
				ImageLoadFileFormat = (ImageLoadFileFormatType)Enum.Parse(typeof(ImageLoadFileFormatType), stats.ImageLoadFileFormat.ToString(), true),
				ImagesToExport = (ImagesToExportType)Enum.Parse(typeof(ImagesToExportType), stats.ImagesToExport.ToString(), true),
				MetadataLoadFileEncodingCodePage = stats.MetadataLoadFileEncodingCodePage,
				MetadataLoadFileFormat = (LoadFileFormat)Enum.Parse(typeof(LoadFileFormat), stats.MetadataLoadFileFormat.ToString(), true),
				MultiValueDelimiter = stats.MultiValueDelimiter,
				NestedValueDelimiter = stats.NestedValueDelimiter,
				NewlineProxy = stats.NewlineProxy,
				OverwriteFiles = stats.OverwriteFiles,
				ProductionPrecedence = stats.ProductionPrecedence,
				RunTimeInMilliseconds = stats.RunTimeInMilliseconds,
				SourceRootFolderID = stats.SourceRootFolderID,
				StartExportAtDocumentNumber = stats.StartExportAtDocumentNumber,
				SubdirectoryImagePrefix = stats.SubdirectoryImagePrefix,
				SubdirectoryMaxFileCount = stats.SubdirectoryMaxFileCount,
				SubdirectoryNativePrefix = stats.SubdirectoryNativePrefix,
				SubdirectoryStartNumber = stats.SubdirectoryStartNumber,
				SubdirectoryTextPrefix = stats.SubdirectoryTextPrefix,
				TextAndNativeFilesNamedAfterFieldID = stats.TextAndNativeFilesNamedAfterFieldID,
				TotalFileBytesExported = stats.TotalFileBytesExported,
				TotalMetadataBytesExported = stats.TotalMetadataBytesExported,
				Type = stats.Type,
				VolumeMaxSize = stats.VolumeMaxSize,
				VolumePrefix = stats.VolumePrefix,
				VolumeStartNumber = stats.VolumeStartNumber,
				WarningCount = stats.WarningCount
			};
		}
	}
}