using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relativity.Sync
{
    internal sealed class TagDocumentsResult<TIdentifier>
    {
        public IEnumerable<TIdentifier> FailedDocuments { get; }

        public string Message { get; }

        public bool Success { get; }

        public int TotalObjectsUpdated { get; }

        public TagDocumentsResult(IEnumerable<TIdentifier> failedDocuments, string message, bool success, int totalObjectsUpdated)
        {
            FailedDocuments = failedDocuments;
            Message = message;
            Success = success;
            TotalObjectsUpdated = totalObjectsUpdated;
        }

        public static TagDocumentsResult<TIdentifier> Empty()
            => new TagDocumentsResult<TIdentifier>(Enumerable.Empty<TIdentifier>(), string.Empty, true, 0);

        public static TagDocumentsResult<TIdentifier> Merge(IEnumerable<TagDocumentsResult<TIdentifier>> tagDocumentsResults)
        {
            List<TIdentifier> failedDocuments = new List<TIdentifier>();
            StringBuilder messageBuilder = new StringBuilder();
            int totalObjectsUpdated = 0;
            bool success = true;

            var results = tagDocumentsResults.ToArray();
            foreach (var result in results)
            {
                failedDocuments.AddRange(result.FailedDocuments);
                messageBuilder.AppendLine(result.Message);
                totalObjectsUpdated += result.TotalObjectsUpdated;
                success &= result.Success;
            }

            return new TagDocumentsResult<TIdentifier>(failedDocuments, messageBuilder.ToString(), success, totalObjectsUpdated);
        }
    }
}
