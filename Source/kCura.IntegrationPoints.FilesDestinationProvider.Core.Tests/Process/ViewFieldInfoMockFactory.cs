using System.Collections.Generic;
using System.Data;
using Relativity;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
	internal static class ViewFieldInfoMockFactory
	{
		public static ViewFieldInfo[] CreateMockedViewFieldInfoArray(List<int> expected,
			bool addFileField = false, int fileFieldId = 0)
		{
			var viewFieldInfo = new List<ViewFieldInfo>();
			DataTable dataTable = CreateMock();
			int elemIndex = 0;
			foreach (var i in expected)
			{
				var row = dataTable.NewRow();
				row["AvfId"] = i;
				if (addFileField && elemIndex++ == 0)
				{
					row["FieldTypeID"] = FieldTypeHelper.FieldType.File;
					row["FieldArtifactID"] = fileFieldId;
				}
				viewFieldInfo.Add(new ViewFieldInfo(row));
			}

			return viewFieldInfo.ToArray();
		}

		private static DataTable CreateMock()
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("FieldArtifactID", typeof(int));
			dataTable.Columns["FieldArtifactID"].DefaultValue = 1;

			dataTable.Columns.Add("AvfID", typeof(int));
			dataTable.Columns["AvfID"].DefaultValue = 1;

			dataTable.Columns.Add("FieldCategoryID", typeof(int));
			dataTable.Columns["FieldCategoryID"].DefaultValue = FieldCategory.Identifier;

			dataTable.Columns.Add("ColumnSource", typeof(string));
			dataTable.Columns["ColumnSource"].DefaultValue = "Computed";

			dataTable.Columns.Add("SourceFieldArtifactTypeID", typeof(int));
			dataTable.Columns["SourceFieldArtifactTypeID"].DefaultValue = 1;

			dataTable.Columns.Add("ConnectorFieldArtifactID", typeof(int));
			dataTable.Columns["ConnectorFieldArtifactID"].DefaultValue = 1;

			dataTable.Columns.Add("FieldTypeID", typeof(int));
			dataTable.Columns["FieldTypeID"].DefaultValue = FieldTypeHelper.FieldType.Empty;

			dataTable.Columns.Add("ConnectorFieldCategoryID", typeof(int));
			dataTable.Columns["ConnectorFieldCategoryID"].DefaultValue = 1;

			dataTable.Columns.Add("IsLinked", typeof(bool));
			dataTable.Columns["IsLinked"].DefaultValue = false;

			dataTable.Columns.Add("FieldCodeTypeID", typeof(int));
			dataTable.Columns["FieldCodeTypeID"].DefaultValue = 1;

			dataTable.Columns.Add("ArtifactTypeID", typeof(int));
			dataTable.Columns["ArtifactTypeID"].DefaultValue = 1;

			dataTable.Columns.Add("FieldIsArtifactBaseField", typeof(bool));
			dataTable.Columns["FieldIsArtifactBaseField"].DefaultValue = false;

			dataTable.Columns.Add("IsUnicodeEnabled", typeof(bool));
			dataTable.Columns["IsUnicodeEnabled"].DefaultValue = false;

			dataTable.Columns.Add("ParentFileFieldArtifactID", typeof(int));
			dataTable.Columns["ParentFileFieldArtifactID"].DefaultValue = 1;

			dataTable.Columns.Add("ParentFileFieldDisplayName", typeof(string));
			dataTable.Columns["ParentFileFieldDisplayName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("AssociativeArtifactTypeID", typeof(int));
			dataTable.Columns["AssociativeArtifactTypeID"].DefaultValue = 1;

			dataTable.Columns.Add("RelationalTableName", typeof(string));
			dataTable.Columns["RelationalTableName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("RelationalTableColumnName", typeof(string));
			dataTable.Columns["RelationalTableColumnName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("RelationalTableColumnName2", typeof(string));
			dataTable.Columns["RelationalTableColumnName2"].DefaultValue = string.Empty;

			dataTable.Columns.Add("SourceFieldArtifactID", typeof(int));
			dataTable.Columns["SourceFieldArtifactID"].DefaultValue = 1;

			dataTable.Columns.Add("EnableDataGrid", typeof(bool));
			dataTable.Columns["EnableDataGrid"].DefaultValue = true;

			dataTable.Columns.Add("DisplayName", typeof(string));
			dataTable.Columns["DisplayName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("AvfColumnName", typeof(string));
			dataTable.Columns["AvfColumnName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("AvfHeaderName", typeof(string));
			dataTable.Columns["AvfHeaderName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("AllowFieldName", typeof(string));
			dataTable.Columns["AllowFieldName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("SourceFieldArtifactTypeTableName", typeof(string));
			dataTable.Columns["SourceFieldArtifactTypeTableName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("ConnectorFieldName", typeof(string));
			dataTable.Columns["ConnectorFieldName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("ArtifactTypeTableName", typeof(string));
			dataTable.Columns["ArtifactTypeTableName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("FormatString", typeof(string));
			dataTable.Columns["FormatString"].DefaultValue = string.Empty;

			dataTable.Columns.Add("DataSource", typeof(string));
			dataTable.Columns["DataSource"].DefaultValue = string.Empty;

			dataTable.Columns.Add("SourceFieldDisplayName", typeof(string));
			dataTable.Columns["SourceFieldDisplayName"].DefaultValue = string.Empty;

			dataTable.Columns.Add("AllowHtml", typeof(bool));
			dataTable.Columns["AllowHtml"].DefaultValue = false;

			return dataTable;
		}
	}
}