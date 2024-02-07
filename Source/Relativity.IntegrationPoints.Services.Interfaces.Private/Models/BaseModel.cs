namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Base class for all models based on RDOs
    /// </summary>
    public class BaseModel
    {
        /// <summary>
        /// Artifact Id of the object
        /// </summary>
        public int ArtifactId { get; set; }

        /// <summary>
        /// Name of the object
        /// </summary>
        public string Name { get; set; }
    }
}
