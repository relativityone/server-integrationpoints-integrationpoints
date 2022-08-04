using System.Data;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
    public class ArgumentMatcher
    {
        public static bool DataTablesMatch(DataTable expected, DataTable actual)
        {
            if (expected == null && actual == null)
            {
                return true;
            }
            if (expected == null && actual != null)
            {
                return false;
            }
            if (expected != null && actual == null)
            {
                return false;
            }

            if (expected.Columns.Count != actual.Columns.Count)
            {
                return false;
            }

            if (expected.Columns.Count == 0 
                && actual.Columns.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < expected.Columns.Count; i++)
            {
                if (!expected.Columns[i].ColumnName.Equals(actual.Columns[i].ColumnName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}