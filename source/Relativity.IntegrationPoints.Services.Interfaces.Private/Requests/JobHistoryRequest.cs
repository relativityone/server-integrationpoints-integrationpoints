namespace Relativity.IntegrationPoints.Services
{
    public class JobHistoryRequest
    {
        /// <summary>
        /// The workspace to retrieve the information from
        /// </summary>
        public int WorkspaceArtifactId { get; set; }

        /// <summary>
        /// The size of the page to request
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The page to request
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The name of the field to sort on
        /// </summary>
        public string SortColumnName { get; set; }

        /// <summary>
        /// Will sort descending if true, ascending otherwise
        /// </summary>
        public bool? SortDescending { get; set; }
    }
}
