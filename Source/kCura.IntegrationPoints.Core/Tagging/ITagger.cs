using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Tagging
{
    public interface ITagger
    {
        void TagDocuments(TagsContainer tagsContainer, IScratchTableRepository scratchTableRepository);
    }
}
