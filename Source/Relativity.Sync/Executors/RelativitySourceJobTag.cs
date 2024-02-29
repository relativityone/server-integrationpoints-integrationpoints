namespace Relativity.Sync.Executors
{
    internal sealed class RelativitySourceJobTag
    {
        public int ArtifactId { get; set; }

        public string Name { get; set; }

        public int JobHistoryArtifactId { get; set; }

        public string JobHistoryName { get; set; }

        public int SourceCaseTagArtifactId { get; set; }
    }
}
