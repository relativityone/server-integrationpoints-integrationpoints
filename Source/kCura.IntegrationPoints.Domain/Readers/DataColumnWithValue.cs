using System.Data;

namespace kCura.IntegrationPoints.Domain.Readers
{
    public class DataColumnWithValue : DataColumn
    {
        public DataColumnWithValue(string name, string value) : base(name)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
