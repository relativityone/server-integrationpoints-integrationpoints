namespace kCura.IntegrationPoints.Services
{
	public class UpdateIntegrationPointRequest : CreateIntegrationPointRequest
	{
		public int ArtifactId { get; set; }

		public override Core.Models.IntegrationPointModel ToModel()
		{
			var model = base.ToModel();
			model.ArtifactID = ArtifactId;
			return model;
		}
	}
}