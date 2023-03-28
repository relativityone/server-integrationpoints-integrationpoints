using Relativity.Services;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class InstanceSettingConditionBuilder
    {
        public static string GetCondition(string name, string section)
        {
            return $"('Name' == '{name}' AND 'Section' == '{section}')";
        }
    }
}
