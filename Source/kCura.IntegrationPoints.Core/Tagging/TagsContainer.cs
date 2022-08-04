using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Tagging
{
    public class TagsContainer
    {
        public SourceJobDTO SourceJobDto { get; }
        public SourceWorkspaceDTO SourceWorkspaceDto { get; }

        public TagsContainer(SourceJobDTO sourceJobDto, SourceWorkspaceDTO sourceWorkspaceDto)
        {
            SourceJobDto = sourceJobDto;
            SourceWorkspaceDto = sourceWorkspaceDto;
        }
    }
}