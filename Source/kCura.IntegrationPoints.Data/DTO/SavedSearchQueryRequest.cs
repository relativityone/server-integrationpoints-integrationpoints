namespace kCura.IntegrationPoints.Data.DTO
{
    public class SavedSearchQueryRequest
    {
        public SavedSearchQueryRequest(string searchTerm, int page, int pageSize)
        {
            SearchTerm = searchTerm;
            Page = page;
            PageSize = pageSize;
        }

        public string SearchTerm { get; }
        public int Page { get; }
        public int PageSize { get; }
    }
}
