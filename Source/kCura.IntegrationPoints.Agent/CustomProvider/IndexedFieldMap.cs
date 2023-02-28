using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    /// <summary>
    /// Wraps FieldMap class from IntegrationPoint.FieldMappings by properties needed for import process with ImportAPI 2.0
    /// </summary>
    internal class IndexedFieldMap
    {
        public IndexedFieldMap(FieldMap fieldMap, int columnIndex)
        {
            FieldMap = fieldMap;
            ColumnIndex = columnIndex;
        }

        public FieldMap FieldMap { get; }

        public int ColumnIndex { get; }

        public string DestinationFieldName => FieldMap.DestinationField.DisplayName;
    }
}
