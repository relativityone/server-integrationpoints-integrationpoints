namespace kCura.IntegrationPoints.Contracts.Models
{
	public class ArtifactFieldDTO
	{
		public int ArtifactId { get; set; }

		public string FieldType { get; set; }

		public string Name { get; set; }

		public virtual object Value { get; set; }
	}
}