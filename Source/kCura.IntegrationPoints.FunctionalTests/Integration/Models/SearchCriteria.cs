namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class SearchCriteria
    {
        public bool HasNatives { get; set; }

        public bool HasImages { get; set; }

        public bool HasFields { get; set; }

        public SearchCriteria()
        {
        }

        public SearchCriteria(bool hasNatives, bool hasImages, bool hasFields)
        {
            HasNatives = hasNatives;
            HasImages = hasImages;
            HasFields = hasFields;
        }
    }
}
