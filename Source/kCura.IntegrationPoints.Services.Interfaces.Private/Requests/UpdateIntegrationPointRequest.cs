using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services
{
	public class UpdateIntegrationPointRequest : CreateIntegrationPointRequest
	{
		public int ArtifactId { get; set; }

		public override Core.Models.IntegrationPointModel ToModel(IList<Choice> choices)
		{
			var model = base.ToModel(choices);
			model.ArtifactID = ArtifactId;
			return model;
		}
	}
}