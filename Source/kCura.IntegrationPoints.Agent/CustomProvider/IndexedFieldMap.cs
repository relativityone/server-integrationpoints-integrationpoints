using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    /// <summary>
    /// Wraps FieldMap class from IntegrationPoint.FieldMappings by properties needed for import process with ImportAPI 2.0
    /// </summary>
    public class IndexedFieldMap
    {
        public FieldMap FieldMap { get; }

        public int ColumnIndex { get; }

        public string DestinationFieldName => FieldMap.DestinationField.DisplayName;

        public FieldMapType FieldMapType { get; }

        public IndexedFieldMap(FieldMap fieldMap, FieldMapType fieldMapType, int columnIndex)
        {
            FieldMap = fieldMap;
            FieldMapType = fieldMapType;
            ColumnIndex = columnIndex;
        }
    }
}
