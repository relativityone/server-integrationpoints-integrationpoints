namespace kCura.IntegrationPoints.Domain.Models
{
    /// <summary>
    /// A data transfer object class used representing a Production.
    /// </summary>
    public class ProductionDTO
    {
        /// <summary>
        /// Gets or sets the name of the production.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the artifact id of the production.
        /// </summary>
        public string ArtifactID { get; set; }
    }
}
