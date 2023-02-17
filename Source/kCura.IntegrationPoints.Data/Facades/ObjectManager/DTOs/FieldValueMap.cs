using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager.DTOs
{
    internal class FieldValueMap
    {
        public string FieldName { get; }

        public object Value { get; }

        public FieldValueMap(FieldValuePair fieldValuePair)
            : this(fieldValuePair.Field.Name, fieldValuePair.Value)
        {
        }

        public FieldValueMap(FieldRefValuePair fieldRefValuePair)
            : this(fieldRefValuePair.Field.Name, fieldRefValuePair.Value)
        {
        }

        private FieldValueMap(string fieldName, object value)
        {
            FieldName = fieldName;
            Value = value;
        }
    }
}
