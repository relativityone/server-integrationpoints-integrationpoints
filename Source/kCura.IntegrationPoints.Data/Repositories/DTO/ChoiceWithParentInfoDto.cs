namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
    public class ChoiceWithParentInfoDto : ChoiceDto
    {
        public int? ParentArtifactID { get; }

        public ChoiceWithParentInfoDto(int? parentArtifactID, int artifactID, string name) : base(artifactID, name)
        {
            ParentArtifactID = parentArtifactID;
        }
    }
}
