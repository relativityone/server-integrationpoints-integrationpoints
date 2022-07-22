using Relativity.API;

namespace Relativity.Sync.Executors
{
    internal sealed class TagNameFormatter : ITagNameFormatter
    {
        private const int _NAME_MAX_LENGTH = 255;
        private readonly IAPILog _logger;

        public TagNameFormatter(IAPILog logger)
        {
            _logger = logger;
        }

        public string FormatWorkspaceDestinationTagName(string federatedInstanceName, string destinationWorkspaceName, int destinationWorkspaceArtifactId)
        {
            string name = GetFormatForWorkspaceOrJobDisplay(federatedInstanceName, destinationWorkspaceName, destinationWorkspaceArtifactId);
            if (name.Length > _NAME_MAX_LENGTH)
            {
                _logger.LogWarning("Relativity Source Case Name exceeded max length and has been shortened.");

                int overflow = name.Length - _NAME_MAX_LENGTH;
                string trimmedInstanceName = federatedInstanceName.Substring(0, federatedInstanceName.Length - overflow);
                name = GetFormatForWorkspaceOrJobDisplay(trimmedInstanceName, destinationWorkspaceName, destinationWorkspaceArtifactId);
            }

            return name;
        }

        public string FormatSourceJobTagName(string jobHistoryName, int jobHistoryArtifactId)
        {
            string name = GetFormatForSourceJobTagName(jobHistoryName, jobHistoryArtifactId);
            if (name.Length > _NAME_MAX_LENGTH)
            {
                _logger.LogWarning("Relativity Source Job Name exceeded max length and has been shortened.");

                int overflow = name.Length - _NAME_MAX_LENGTH;
                string trimmedJobHistoryName = jobHistoryName.Substring(0, jobHistoryName.Length - overflow);
                name = GetFormatForSourceJobTagName(trimmedJobHistoryName, jobHistoryArtifactId);
            }
            return name;
        }

        public string FormatSourceCaseTagName(string instanceName, string sourceWorkspaceName, int workspaceArtifactId)
        {
            string name = GetFormatForSourceCaseTagName(instanceName, sourceWorkspaceName, workspaceArtifactId);
            if (name.Length > _NAME_MAX_LENGTH)
            {
                _logger.LogWarning("Relativity Source Case Name exceeded max length and has been shortened.");

                int overflow = name.Length - _NAME_MAX_LENGTH;
                string trimmedInstanceName = instanceName.Substring(0, instanceName.Length - overflow);
                name = GetFormatForSourceCaseTagName(trimmedInstanceName, sourceWorkspaceName, workspaceArtifactId);
            }
            return name;
        }

        private static string GetFormatForWorkspaceOrJobDisplay(string prefix, string name, int? id)
        {
            return $"{prefix} - {GetFormatForWorkspaceOrJobDisplay(name, id)}";
        }

        private static string GetFormatForWorkspaceOrJobDisplay(string name, int? id)
        {
            return id.HasValue ? $"{name} - {id}" : name;
        }

        private static string GetFormatForSourceJobTagName(string jobHistoryName, int jobHistoryArtifactId)
        {
            return $"{jobHistoryName} - {jobHistoryArtifactId}";
        }

        private static string GetFormatForSourceCaseTagName(string instanceName, string workspaceName, int workspaceArtifactId)
        {
            return $"{instanceName} - {workspaceName} - {workspaceArtifactId}";
        }
    }
}