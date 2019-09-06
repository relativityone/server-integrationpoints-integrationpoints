using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to update the Integration Point Profiles after workspace creation.")]
	[Guid("DC9F2F04-5095-4FAC-96A5-7D8A213A1463")]
	public class IntegrationPointProfileMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => "Failed to migrate the Integration Point Profiles.";

		protected override void Run()
		{
			// Skeleton method to be implemented in REL-351467
		}
	}
}