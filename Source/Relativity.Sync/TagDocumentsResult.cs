using System.Collections.Generic;

namespace Relativity.Sync
{
	internal sealed class TagDocumentsResult<T>
	{
		public IEnumerable<T> FailedDocuments { get; }

		public string Message { get; }

		public bool Success { get; }

		public int TotalObjectsUpdated { get; }

		public TagDocumentsResult(IEnumerable<T> failedDocuments, string message, bool success, int totalObjectsUpdated)
		{
			FailedDocuments = failedDocuments;
			Message = message;
			Success = success;
			TotalObjectsUpdated = totalObjectsUpdated;
		}
	}
}