using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
    public class ChoiceWithChildInfoDto : ChoiceDto
    {
        public IList<ChoiceWithChildInfoDto> Children { get; }

        public ChoiceWithChildInfoDto(int artifactID, string name, IList<ChoiceWithChildInfoDto> children) : base(artifactID, name)
        {
            Children = children;
        }
    }
}
