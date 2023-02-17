namespace kCura.IntegrationPoints.Domain.Models
{
    public class SavedSearchDTO : BaseDTO
    {
        public int ParentContainerId { get; set; }

        public string Owner { get; set; }

        public bool IsPublic => string.IsNullOrEmpty(Owner);
    }
}
