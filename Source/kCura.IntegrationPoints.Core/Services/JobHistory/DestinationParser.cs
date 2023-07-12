using System;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class DestinationParser : IDestinationParser
    {
        private const char _SEPARATOR = '-';
        private readonly string parsingError =
            "Destination workspace object: {0} could not be parsed.";

        public int GetArtifactId(string destinationWorkspace)
        {
            try
            {
                int workspaceArtifactIdStartIndex = destinationWorkspace.LastIndexOf(_SEPARATOR) + 1;
                int workspaceArtifactIdEndIndex = destinationWorkspace.Length;
                string workspaceArtifactIdSubstring = destinationWorkspace.Substring(workspaceArtifactIdStartIndex, workspaceArtifactIdEndIndex - workspaceArtifactIdStartIndex);
                int workspaceArtifactId = int.Parse(workspaceArtifactIdSubstring);

                return workspaceArtifactId;
            }
            catch (Exception e)
            {
                throw CreateWrongFormatException(destinationWorkspace, e);
            }
        }

        private Exception CreateWrongFormatException(string destinationWorkspace, Exception e)
        {
            return new Exception(string.Format(parsingError, destinationWorkspace), e);
        }
    }
}
