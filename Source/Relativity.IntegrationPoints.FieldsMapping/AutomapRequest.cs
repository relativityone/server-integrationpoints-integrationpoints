namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class AutomapRequest
	{
		public DocumentFieldInfo[] SourceFields { get; set; }
		public DocumentFieldInfo[] DestinationFields { get; set; }
		public bool MatchOnlyIdentifiers { get; set; }
	}
}