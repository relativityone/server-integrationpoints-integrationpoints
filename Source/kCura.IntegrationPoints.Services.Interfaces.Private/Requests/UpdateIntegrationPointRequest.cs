using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace kCura.IntegrationPoints.Services
{
	public class UpdateIntegrationPointRequest : CreateIntegrationPointRequest
	{
		public int IntegrationPointArtifactId { get; set; }

		public override void ValidateRequest()
		{
			if (IntegrationPointArtifactId < 1)
			{
				throw new Exception($"Invalid integration point id found : {IntegrationPointArtifactId}");
			}
			base.ValidateRequest();
		}

		public override void ValidatePermission(IWindsorContainer container)
		{
			IIntegrationPointService service = container.Resolve<IIntegrationPointService>();
			if (service.GetRdo(IntegrationPointArtifactId) == null)
			{
				throw new Exception($"Unable to find requested integration point : {IntegrationPointArtifactId}");
			}
			base.ValidatePermission(container);
		}

		public override Core.Models.IntegrationPointModel CreateIntegrationPointModel(IWindsorContainer container)
		{
			Core.Models.IntegrationPointModel baseModel = base.CreateIntegrationPointModel(container);
			baseModel.ArtifactID = IntegrationPointArtifactId;
			return baseModel;
		}
	}
}