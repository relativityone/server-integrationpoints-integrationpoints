using System;
using Relativity;
using Relativity.Services.Interfaces.ViewField.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
	internal class CoreViewFieldInfo : ViewFieldInfo
	{
		public CoreViewFieldInfo(ViewFieldResponse viewFieldResponse)
		{
			Initialize(viewFieldResponse);
		}

		private void Initialize(ViewFieldResponse viewFieldResponse)
		{
			FieldArtifactId = viewFieldResponse.ArtifactID;
			AvfId = viewFieldResponse.ArtifactViewFieldID;
			Category = ConvertEnum<FieldCategory>(viewFieldResponse.Category);
			DisplayName = viewFieldResponse.DisplayName;
			AvfColumnName = viewFieldResponse.ArtifactViewFieldColumnName;
			AvfHeaderName = viewFieldResponse.ArtifactViewFieldHeaderName;
			AllowFieldName = viewFieldResponse.AllowFieldName;
			ColumnSource = ConvertEnum<ColumnSourceType>(viewFieldResponse.ColumnSourceType);
			DataSource = viewFieldResponse.DataSource;
			SourceFieldName = viewFieldResponse.SourceFieldName;
			SourceFieldArtifactTypeID = viewFieldResponse.SourceFieldArtifactTypeID;
			SourceFieldArtifactID = viewFieldResponse.SourceFieldArtifactID;
			ConnectorFieldArtifactID = viewFieldResponse.ConnectorFieldArtifactID;
			_sourceFieldArtifactTypeTableName = viewFieldResponse.SourceFieldArtifactTypeTableName;
			ConnectorFieldName = viewFieldResponse.ConnectorFieldName;
			_connectorFieldCategory = ConvertEnum<FieldCategory>(viewFieldResponse.ConnectorFieldCategory);
			FieldType = ConvertEnum<FieldTypeHelper.FieldType>(viewFieldResponse.FieldType);
			IsLinked = viewFieldResponse.IsLinked;
			FieldCodeTypeID = viewFieldResponse.FieldCodeTypeID;
			ArtifactTypeID = viewFieldResponse.ArtifactTypeID;
			ArtifactTypeTableName = viewFieldResponse.ArtifactTypeTableName;
			FieldIsArtifactBaseField = viewFieldResponse.FieldIsArtifactBaseField;
			FormatString = viewFieldResponse.FormatString;
			IsUnicodeEnabled = viewFieldResponse.IsUnicodeEnabled;
			AllowHtml = viewFieldResponse.AllowHtml;
			ParentFileFieldArtifactID = viewFieldResponse.ParentFileFieldArtifactID;
			ParentFileFieldDisplayName = viewFieldResponse.ParentFileFieldDisplayName;
			AssociativeArtifactTypeID = viewFieldResponse.AssociativeArtifactTypeID;
			RelationalTableName = viewFieldResponse.RelationalTableName;
			RelationalTableColumnName = viewFieldResponse.RelationalTableColumnName;
			RelationalTableColumnName2 = viewFieldResponse.RelationalTableColumnName2;
			ParentReflectionType = ConvertEnum<ParentReflectionType>(viewFieldResponse.ParentReflectionType);
			ReflectedFieldArtifactTypeTableName = viewFieldResponse.ReflectedFieldArtifactTypeTableName;
			ReflectedFieldIdentifierColumnName = viewFieldResponse.ReflectedFieldIdentifierColumnName;
			ReflectedFieldConnectorFieldName = viewFieldResponse.ReflectedFieldConnectorFieldName;
			ReflectedConnectorIdentifierColumnName = viewFieldResponse.ReflectedConnectorIdentifierColumnName;
			EnableDataGrid = viewFieldResponse.EnableDataGrid;
			IsVirtualAssociativeArtifactType = viewFieldResponse.IsVirtualAssociativeArtifactType;
		}

		private static TEnum ConvertEnum<TEnum>(Enum source)
		{
			if (source == null)
			{
				return default(TEnum);
			}

			return (TEnum) Enum.Parse(typeof(TEnum), source.ToString(), true);
		}
	}
}
