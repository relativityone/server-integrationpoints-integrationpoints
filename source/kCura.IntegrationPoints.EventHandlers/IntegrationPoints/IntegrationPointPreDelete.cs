using System;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoint
{
	[kCura.EventHandler.CustomAttributes.Description("A description of the event handler.")]
	[System.Runtime.InteropServices.Guid("4ee07342-b4a0-45a3-a6a3-b56cc739feb7")]
	public class IntegrationPointPreDelete : kCura.EventHandler.PreMassDeleteEventHandler
	{
		public override void Commit()
		{}

		public override EventHandler.Response Execute()
		{
			var integrationPointArtifactId = ActiveArtifact.ArtifactID;

			return null;
		}

		public override EventHandler.FieldCollection RequiredFields
		{
			get { return new EventHandler.FieldCollection(); }
		}

		public override void Rollback()
		{
			throw new NotImplementedException();
		}
	}
}
