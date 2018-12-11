using System;
using kCura.EDDS.WebAPI.AuditManagerBase;
using kCura.EDDS.WebAPI.ExportManagerBase;
using kCura.EDDS.WebAPI.FieldManagerBase;
using kCura.EDDS.WebAPI.ProductionManagerBase;
using Relativity.Core;

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

		public static global::Relativity.API.Foundation.ExportStatistics ToFoundationExportStatistics(this ExportStatistics exportStatistics)
		{
			if (exportStatistics == null)
			{
				return null;
			}
			var foundationExportStatistics = new global::Relativity.API.Foundation.ExportStatistics();
			foundationExportStatistics.Type = exportStatistics.Type;
			foundationExportStatistics.Fields = exportStatistics.Fields;
			foundationExportStatistics.DestinationFilesystemFolder = exportStatistics.DestinationFilesystemFolder;
			foundationExportStatistics.OverwriteFiles = exportStatistics.OverwriteFiles;
			foundationExportStatistics.VolumePrefix = exportStatistics.VolumePrefix;
			foundationExportStatistics.VolumeMaxSize = exportStatistics.VolumeMaxSize;
			foundationExportStatistics.SubdirectoryImagePrefix = exportStatistics.SubdirectoryImagePrefix;
			foundationExportStatistics.SubdirectoryNativePrefix = exportStatistics.SubdirectoryNativePrefix;
			foundationExportStatistics.SubdirectoryTextPrefix = exportStatistics.SubdirectoryTextPrefix;
			foundationExportStatistics.SubdirectoryStartNumber = exportStatistics.SubdirectoryStartNumber;
			foundationExportStatistics.SubdirectoryMaxFileCount = exportStatistics.SubdirectoryMaxFileCount;
			foundationExportStatistics.FilePathSettings = exportStatistics.FilePathSettings;
			foundationExportStatistics.Delimiter = exportStatistics.Delimiter;
			foundationExportStatistics.Bound = exportStatistics.Bound;
			foundationExportStatistics.NewlineProxy = exportStatistics.NewlineProxy;
			foundationExportStatistics.MultiValueDelimiter = exportStatistics.MultiValueDelimiter;
			foundationExportStatistics.NestedValueDelimiter = exportStatistics.NestedValueDelimiter;
			foundationExportStatistics.TextAndNativeFilesNamedAfterFieldID = exportStatistics.TextAndNativeFilesNamedAfterFieldID;
			foundationExportStatistics.AppendOriginalFilenames = exportStatistics.AppendOriginalFilenames;
			foundationExportStatistics.ExportImages = exportStatistics.ExportImages;
			foundationExportStatistics.ImageLoadFileFormat = ConvertToFoundationImageLoadFileFormatType(exportStatistics.ImageLoadFileFormat);
			foundationExportStatistics.ImageFileType = ConvertToFoundationImageFileExportType(exportStatistics.ImageFileType);
			foundationExportStatistics.ExportNativeFiles = exportStatistics.ExportNativeFiles;
			foundationExportStatistics.MetadataLoadFileFormat = ConvertToFoundationLoadFileFormat(exportStatistics.MetadataLoadFileFormat);
			foundationExportStatistics.MetadataLoadFileEncodingCodePage = exportStatistics.MetadataLoadFileEncodingCodePage;
			foundationExportStatistics.ExportTextFieldAsFiles = exportStatistics.ExportTextFieldAsFiles;
			foundationExportStatistics.ExportedTextFileEncodingCodePage = exportStatistics.ExportedTextFileEncodingCodePage;
			foundationExportStatistics.ExportedTextFieldID = exportStatistics.ExportedTextFieldID;
			foundationExportStatistics.ExportMultipleChoiceFieldsAsNested = exportStatistics.ExportMultipleChoiceFieldsAsNested;
			foundationExportStatistics.TotalFileBytesExported = exportStatistics.TotalFileBytesExported;
			foundationExportStatistics.TotalMetadataBytesExported = exportStatistics.TotalMetadataBytesExported;
			foundationExportStatistics.ErrorCount = exportStatistics.ErrorCount;
			foundationExportStatistics.WarningCount = exportStatistics.WarningCount;
			foundationExportStatistics.DocumentExportCount = exportStatistics.DocumentExportCount;
			foundationExportStatistics.FileExportCount = exportStatistics.FileExportCount;
			foundationExportStatistics.RunTimeInMilliseconds = exportStatistics.RunTimeInMilliseconds;
			foundationExportStatistics.ImagesToExport = ConvertToFoundationImagesToExportType(exportStatistics.ImagesToExport);
			foundationExportStatistics.ProductionPrecedence = exportStatistics.ProductionPrecedence;
			foundationExportStatistics.DataSourceArtifactID = exportStatistics.DataSourceArtifactID;
			foundationExportStatistics.SourceRootFolderID = exportStatistics.SourceRootFolderID;
			foundationExportStatistics.CopyFilesFromRepository = exportStatistics.CopyFilesFromRepository;
			foundationExportStatistics.StartExportAtDocumentNumber = exportStatistics.StartExportAtDocumentNumber;
			foundationExportStatistics.VolumeStartNumber = exportStatistics.VolumeStartNumber;
			foundationExportStatistics.ArtifactTypeID = exportStatistics.ArtifactTypeID;
			return foundationExportStatistics;
		}

		private static global::Relativity.API.Foundation.ExportStatistics.ImageLoadFileFormatType ConvertToFoundationImageLoadFileFormatType(ImageLoadFileFormatType imageLoadFileFormatType)
		{
			switch (imageLoadFileFormatType)
			{
				case ImageLoadFileFormatType.Opticon:
					return global::Relativity.API.Foundation.ExportStatistics.ImageLoadFileFormatType.Opticon;
				case ImageLoadFileFormatType.Ipro:
					return global::Relativity.API.Foundation.ExportStatistics.ImageLoadFileFormatType.Ipro;
				default:
					return global::Relativity.API.Foundation.ExportStatistics.ImageLoadFileFormatType.IproFullText;
			}
		}

		private static global::Relativity.API.Foundation.ExportStatistics.ImageFileExportType ConvertToFoundationImageFileExportType(ImageFileExportType imageFileExportType)
		{
			switch (imageFileExportType)
			{
				case ImageFileExportType.SinglePage:
					return global::Relativity.API.Foundation.ExportStatistics.ImageFileExportType.SinglePage;
				case ImageFileExportType.MultiPageTiff:
					return global::Relativity.API.Foundation.ExportStatistics.ImageFileExportType.MultiPageTiff;
				default:
					return global::Relativity.API.Foundation.ExportStatistics.ImageFileExportType.PDF;
			}
		}

		private static global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat ConvertToFoundationLoadFileFormat(LoadFileFormat loadFileFormat)
		{
			switch (loadFileFormat)
			{
				case LoadFileFormat.Csv:
					return global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat.Csv;
				case LoadFileFormat.Dat:
					return global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat.Dat;
				case LoadFileFormat.Custom:
					return global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat.Custom;
				default:
					return global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat.Html;
			}
		}

		private static global::Relativity.API.Foundation.ExportStatistics.ImagesToExportType ConvertToFoundationImagesToExportType(ImagesToExportType imagesToExportType)
		{
			switch (imagesToExportType)
			{
				case ImagesToExportType.Original:
					return global::Relativity.API.Foundation.ExportStatistics.ImagesToExportType.Original;
				case ImagesToExportType.Produced:
					return global::Relativity.API.Foundation.ExportStatistics.ImagesToExportType.Produced;
				default:
					return global::Relativity.API.Foundation.ExportStatistics.ImagesToExportType.Both;
			}
		}
	}
}