using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using kCura.Relativity.Export.Exports;
using kCura.Relativity.Export.Types;
using Relativity;
using ViewFieldInfo = Relativity.ViewFieldInfo;

namespace kCura.Relativity.Export.FileObjects
{


    public static class ExportFileDefBuilder
    {
		private static readonly DataTable _dataTable  = new DataTable();

		static ExportFileDefBuilder()
		{
			_dataTable.Columns.Add("FieldArtifactID");
			_dataTable.Columns.Add("AvfID");
			_dataTable.Columns.Add("FieldCategoryID");
			_dataTable.Columns.Add("DisplayName");
			_dataTable.Columns.Add("AvfColumnName");
			_dataTable.Columns.Add("AvfHeaderName");
			_dataTable.Columns.Add("AllowFieldName");
			_dataTable.Columns.Add("ColumnSource");
			_dataTable.Columns.Add("DataSource");
			_dataTable.Columns.Add("SourceFieldDisplayName");
			_dataTable.Columns.Add("SourceFieldArtifactTypeID");
			_dataTable.Columns.Add("ConnectorFieldArtifactID");
			_dataTable.Columns.Add("SourceFieldArtifactTypeTableName");
			_dataTable.Columns.Add("ConnectorFieldName");
			_dataTable.Columns.Add("FieldTypeID");
			_dataTable.Columns.Add("ConnectorFieldCategoryID");
			_dataTable.Columns.Add("IsLinked");
			_dataTable.Columns.Add("FieldCodeTypeID");
			_dataTable.Columns.Add("ArtifactTypeID");
			_dataTable.Columns.Add("ArtifactTypeTableName");
			_dataTable.Columns.Add("FieldIsArtifactBaseField");
			_dataTable.Columns.Add("FormatString");
			_dataTable.Columns.Add("IsUnicodeEnabled");
			_dataTable.Columns.Add("ParentFileFieldArtifactID");
			_dataTable.Columns.Add("ParentFileFieldDisplayName");
			_dataTable.Columns.Add("AssociativeArtifactTypeID");
			_dataTable.Columns.Add("RelationalTableName");
			_dataTable.Columns.Add("RelationalTableColumnName");
			_dataTable.Columns.Add("RelationalTableColumnName2");
			_dataTable.Columns.Add("SourceFieldArtifactID");
			_dataTable.Columns.Add("EnableDataGrid");
		}

		public static ExportFile CreateDefSetup(int exportedObjArtifactId, int workspaceId, string password, string userName, string exportFilesLocation, 
			List<int> selViewFieldIds, int artifactTypeId = 10)
        {

            ExportFile expFile = new ExportFile(artifactTypeId);

            expFile.AppendOriginalFileName = false;
            expFile.ArtifactID = exportedObjArtifactId;
            expFile.CaseInfo = new CaseInfo();
            expFile.CaseInfo.ArtifactID = workspaceId;
			

			expFile.CookieContainer = new System.Net.CookieContainer();
			expFile.Credential = new NetworkCredential();
            expFile.Credential.Password = password;
            expFile.Credential.UserName = userName;
            expFile.ExportFullText = false;
            expFile.ExportImages = true;
            expFile.ExportFullTextAsFile = false;
            expFile.ExportNative = true;
            expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;
            expFile.FilePrefix = "";
            expFile.FolderPath = exportFilesLocation;
            expFile.IdentifierColumnName = "Control Number";

            List<Pair> imagePrecs = new List<Pair>();
            imagePrecs.Add(new Pair("-1", "Original"));

            expFile.ImagePrecedence = imagePrecs.ToArray();
            expFile.LoadFileEncoding = System.Text.Encoding.Default;
            expFile.LoadFileExtension = "dat";
            expFile.LoadFileIsHtml = false;
            expFile.LoadFilesPrefix = "Extracted Text Only";
            expFile.LogFileFormat = LoadFileType.FileFormat.Opticon;
            expFile.ObjectTypeName = "Document";
            expFile.Overwrite = true;
            expFile.RenameFilesToIdentifier = true;
			expFile.SelectedViewFields = selViewFieldIds.Select(CreateViewFieldInfo).ToArray();

			expFile.StartAtDocumentNumber = 0;
            expFile.SubdirectoryDigitPadding = 3;
            expFile.TextFileEncoding = null;
            expFile.TypeOfExport = ExportFile.ExportType.ArtifactSearch;

            expFile.TypeOfExportedFilePath = ExportFile.ExportedFilePathType.Relative;

            expFile.TypeOfImage = ExportFile.ImageType.SinglePage;
            expFile.ViewID = 0;
            expFile.VolumeDigitPadding = 2;
            expFile.VolumeInfo = new VolumeInfo();
            expFile.VolumeInfo.VolumePrefix = "VOL";
            expFile.VolumeInfo.VolumeStartNumber = 1;
            expFile.VolumeInfo.VolumeMaxSize = 650;
            expFile.VolumeInfo.SubdirectoryStartNumber = 1;
            expFile.VolumeInfo.SubdirectoryMaxSize = 500;
            expFile.VolumeInfo.CopyFilesFromRepository = true;

            return expFile;
        }

	    private static Types.ViewFieldInfo CreateViewFieldInfo(int fieldId)
	    {
		    DataRow row = _dataTable.NewRow();

		    row["FieldArtifactID"] = fieldId;
		    row["AvfID"] = 0;
		    row["FieldCategoryID"] = "0";
		    row["DisplayName"] = "";
		    row["AvfColumnName"] = "";
		    row["AvfHeaderName"] = "";
		    row["AllowFieldName"] = "";
		    row["ColumnSource"] = ViewFieldInfo.ColumnSourceType.Artifact.ToString();
		    row["DataSource"] = "";
		    row["SourceFieldDisplayName"] = "";
		    row["SourceFieldArtifactTypeID"] = "0";
		    row["ConnectorFieldArtifactID"] = 0;
		    row["SourceFieldArtifactTypeTableName"] = "";
		    row["ConnectorFieldName"] = "";
		    row["FieldTypeID"] = "0";
		    row["ConnectorFieldCategoryID"] = "0";
		    row["IsLinked"] = "false";
		    row["FieldCodeTypeID"] = "0";
		    row["ArtifactTypeID"] = "0";
		    row["ArtifactTypeTableName"] = "";
		    row["FieldIsArtifactBaseField"] = "false";
		    row["FormatString"] = "";
		    row["IsUnicodeEnabled"] = "false";
		    row["ParentFileFieldArtifactID"] = "0";
		    row["ParentFileFieldDisplayName"] = "";
		    row["AssociativeArtifactTypeID"] = "0";
		    row["RelationalTableName"] = "";
		    row["RelationalTableColumnName"] = "";
		    row["RelationalTableColumnName2"] = "";
		    row["SourceFieldArtifactID"] = "0";
		    row["EnableDataGrid"] = "false";

		    Types.ViewFieldInfo fieldInfo = new Types.ViewFieldInfo(new ViewFieldInfo(row));

			return fieldInfo;
	    }

    }


}
