namespace kCura.IntegrationPoints.UITests.Configuration.Models
{
    public class FieldDisplayNamePair
    {
        public string SourceDisplayName { get; set; }
        public string DestinationDisplayName { get; set; }

        public FieldDisplayNamePair(string sourceDisplayName, string destinationDisplayName)
        {
            SourceDisplayName = sourceDisplayName;
            DestinationDisplayName = destinationDisplayName;
        }

        public FieldDisplayNamePair(FieldMapModel fieldPair)
        {
            SourceDisplayName = fieldPair.SourceFieldObject.DisplayName;
            DestinationDisplayName = fieldPair.DestinationFieldObject.DisplayName;
        }
    }
}
