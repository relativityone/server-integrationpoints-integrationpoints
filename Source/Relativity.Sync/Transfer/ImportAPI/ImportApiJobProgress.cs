namespace Relativity.Sync.Transfer.ImportAPI
{
    internal sealed class ImportApiJobProgress
    {
        public ImportApiJobProgress(long completedItem)
        {
            CompletedItem = completedItem;
        }

        public long CompletedItem { get; }
    }
}
