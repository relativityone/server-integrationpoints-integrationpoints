namespace Relativity.Sync.Executors
{
    internal sealed class SnapshotPart
    {
        public SnapshotPart(int startingIndex, int numberOfRecords)
        {
            StartingIndex = startingIndex;
            NumberOfRecords = numberOfRecords;
        }

        public int StartingIndex { get; }
        public int NumberOfRecords { get; }
    }
}