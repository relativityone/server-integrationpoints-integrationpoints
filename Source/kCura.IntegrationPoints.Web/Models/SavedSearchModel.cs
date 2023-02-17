using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Web.Models
{
    public class SavedSearchModel
    {
        public SavedSearchModel(SavedSearchDTO dto) : this()
        {
            DisplayName = dto.Name;
            Value = dto.ArtifactId;
        }

        public SavedSearchModel()
        {
        }

        public int Value { get; set; }

        public string DisplayName { get; set; }
    }
}
