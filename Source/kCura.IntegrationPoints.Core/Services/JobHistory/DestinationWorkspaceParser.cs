using System;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class DestinationWorkspaceParser : IDestinationWorkspaceParser
	{
		private const char _SEPARATOR = '-';

		private readonly string parsingError =
			"The formatting of the destination workspace information has changed and cannot be parsed.";

		public int GetWorkspaceArtifactId(string destinationWorkspace)
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
				throw new Exception(parsingError, e);
			}
		}

		public string GetInstanceName(string destinationWorkspace)
		{
			try
			{
				string[] destinationWorkspaceElements = GetElements(destinationWorkspace);
				return destinationWorkspaceElements[0].Trim();
			}
			catch (Exception e)
			{
				throw new Exception(parsingError, e);
			}
		}

		public string[] GetElements(string destinationWorkspace)
		{
			return destinationWorkspace.Split(_SEPARATOR);
		}
	}
}