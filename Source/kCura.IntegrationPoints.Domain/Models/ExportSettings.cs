﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Domain.Models
{
	public class ExportSettings
	{
		public int SourceWorkspaceArtifactId { set; get; }
		public int TargetWorkspaceArtifactId { get; set; }
		public string TargetWorkspace { get; set; }
		public string SourceWorkspace { get; set; }
	}

	public class ExportUsingSavedSearchSettings : ExportSettings
	{
		public int SavedSearchArtifactId { set; get; }
		public string SavedSearch { set; get; }
		public int StartExportAtRecord { get; set; }
		public string Fileshare { get; set; }
		public bool ExportNatives { get; set; }
		public bool OverwriteFiles { get; set; }
		public bool ExportImages { get; set; }
		public string SelectedImageFileType { get; set; }
		public string SelectedDataFileFormat { get; set; }
		public string DataFileEncodingType { get; set; }
		public string SelectedImageDataFileFormat { get; set; }
        public char ColumnSeparator { get; set; }
        public char QuoteSeparator { get; set; }
        public char NewlineSeparator { get; set; }
        public char MultiValueSeparator { get; set; }
        public char NestedValueSeparator { get; set; }
		public string SubdirectoryImagePrefix { get; set; }
		public string SubdirectoryNativePrefix { get; set; }
		public string SubdirectoryTextPrefix { get; set; }
		public int SubdirectoryStartNumber { get; set; }
		public int SubdirectoryDigitPadding { get; set; }
		public int SubdirectoryMaxFiles { get; set; }
		public string VolumePrefix { get; set; }
		public int VolumeStartNumber { get; set; }
		public int VolumeDigitPadding { get; set; }
		public int VolumeMaxSize { get; set; }
		public string FilePath { get; set; }
		public string UserPrefix { get; set; }
		public bool ExportMultipleChoiceFieldsAsNested { get; set; }
		public bool IncludeNativeFilesPath { get; set; }
		public bool ExportFullTextAsFile { get; set; }
		public IEnumerable<FieldEntry> TextPrecedenceFields { get; set; }
		public string TextFileEncodingType { get; set; }
		public string ProductionPrecedence { get; set; }
		public bool IncludeOriginalImages { get; set; }
		public IEnumerable<ProductionPrecedenceDTO> ImagePrecedence { get; set; }
	}
}