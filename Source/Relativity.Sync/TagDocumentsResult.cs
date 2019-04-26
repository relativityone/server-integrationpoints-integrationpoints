using System.Collections.Generic;

namespace Relativity.Sync
{
	internal sealed class TagDocumentsResult
	{
		public IEnumerable<int> FailedDocumentArtifactIds { get; }

		public string Message { get; }

		public bool Success { get; }

		public int TotalObjectsUpdated { get; }

		public TagDocumentsResult(IEnumerable<int> failedDocumentArtifactIds, string message, bool success, int totalObjectsUpdated)
		{
			FailedDocumentArtifactIds = failedDocumentArtifactIds;
			Message = message;
			Success = success;
			TotalObjectsUpdated = totalObjectsUpdated;
		}
	}
}