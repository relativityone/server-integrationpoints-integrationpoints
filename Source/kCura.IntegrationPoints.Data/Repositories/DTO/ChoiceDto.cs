namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
    public class ChoiceDto
    {
        public int ArtifactID { get; }
        public string Name { get; }

        public ChoiceDto(int artifactID, string name)
        {
            ArtifactID = artifactID;
            Name = name;
        }
    }
}
