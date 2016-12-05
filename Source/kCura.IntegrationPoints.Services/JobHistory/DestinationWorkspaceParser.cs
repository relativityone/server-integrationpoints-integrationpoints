using System;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class DestinationWorkspaceParser : IDestinationWorkspaceParser
	{
		private const string _SEPARATOR = "-";

		public int GetWorkspaceArtifactId(string destinationWorkspace)
		{
			try
			{
				int workspaceArtifactIdStartIndex = destinationWorkspace.LastIndexOf(_SEPARATOR, StringComparison.CurrentCulture) + _SEPARATOR.Length;
				int workspaceArtifactIdEndIndex = destinationWorkspace.Length;
				string workspaceArtifactIdSubstring = destinationWorkspace.Substring(workspaceArtifactIdStartIndex, workspaceArtifactIdEndIndex - workspaceArtifactIdStartIndex);
				int workspaceArtifactId = int.Parse(workspaceArtifactIdSubstring);

				return workspaceArtifactId;
			}
			catch (Exception e)
			{
				throw new Exception("The formatting of the destination workspace information has changed and cannot be parsed.", e);
			}
		}
	}
}