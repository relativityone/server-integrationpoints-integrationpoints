namespace kCura.IntegrationPoints.Domain.Utils
{
    public static class WorkspaceAndJobNameUtils
    {
        public static string GetFormatForWorkspaceOrJobDisplay(string name, int? id)
        {
            return id.HasValue ? $"{name} - {id}" : name;
        }

        public static string GetFormatForWorkspaceOrJobDisplay(string prefix, string name, int? id)
        {
            return $"{prefix} - {GetFormatForWorkspaceOrJobDisplay(name, id)}";
        }
    }
}