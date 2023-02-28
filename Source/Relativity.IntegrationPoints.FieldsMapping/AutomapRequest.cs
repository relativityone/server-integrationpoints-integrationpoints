namespace Relativity.IntegrationPoints.FieldsMapping
{
    public class AutomapRequest
    {
        public FieldInfo[] SourceFields { get; set; }

        public FieldInfo[] DestinationFields { get; set; }

        public bool MatchOnlyIdentifiers { get; set; }
    }
}
