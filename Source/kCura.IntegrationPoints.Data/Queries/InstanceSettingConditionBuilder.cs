using Relativity.Services;

namespace kCura.IntegrationPoints.Data.Queries
{
    public abstract class InstanceSettingConditionBuilder
    {
        public static string GetCondition(string name, string section)
        {
            return $"('Name' == '{name}' AND 'Section' == '{section}')";
        }
    }
}
