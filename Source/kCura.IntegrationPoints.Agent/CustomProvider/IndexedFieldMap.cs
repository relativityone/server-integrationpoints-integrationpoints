using System;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    /// <summary>
    /// Wraps FieldMap class from IntegrationPoint.FieldMappings by properties needed for import process with ImportAPI 2.0
    /// </summary>
    public class IndexedFieldMap
    {
        public IndexedFieldMap(FieldMap fieldMap, int columnIndex)
        {
            FieldMap = fieldMap;
            ColumnIndex = columnIndex;
        }

        public FieldMap FieldMap { get; }

        public int ColumnIndex { get; }

        public string DestinationFieldName => FieldMap.DestinationField.DisplayName;

        public IndexedFieldMap Clone()
        {
            var clonedFieldMap = new FieldMap
            {
                SourceField = new FieldEntry
                {
                    DisplayName = FieldMap.SourceField.DisplayName,
                    IsIdentifier = FieldMap.SourceField.IsIdentifier,
                    IsRequired = FieldMap.SourceField.IsRequired,
                    FieldIdentifier = FieldMap.SourceField.FieldIdentifier,
                    Type = FieldMap.SourceField.Type,
                    FieldType = FieldMap.SourceField.FieldType,
                },
                DestinationField = new FieldEntry
                {
                    DisplayName = FieldMap.DestinationField.DisplayName,
                    IsIdentifier = FieldMap.DestinationField.IsIdentifier,
                    IsRequired = FieldMap.DestinationField.IsRequired,
                    FieldIdentifier = FieldMap.DestinationField.FieldIdentifier,
                    Type = FieldMap.DestinationField.Type,
                    FieldType = FieldMap.DestinationField.FieldType,
                },
                FieldMapType = FieldMap.FieldMapType
            };

            return new IndexedFieldMap(clonedFieldMap, ColumnIndex);
        }
    }
}
