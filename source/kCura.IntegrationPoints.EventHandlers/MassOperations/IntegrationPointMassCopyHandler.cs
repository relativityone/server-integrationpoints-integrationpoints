using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Data;
using kCura.MassOperationHandlers;

namespace kCura.IntegrationPoints.EventHandlers.MassOperations
{
	[Guid("837D85BC-B0C4-4A82-9645-1C7171B922AB")]
	[Description("Integration Point - Mass Copy")]
	public class IntegrationPointMassCopyHandler : MassOperationHandler
	{
		public override Response DoBatch()
		{
			IntegrationPointMassCopy massCopy;
			try
			{
				IRSAPIService service = new RSAPIService(Helper, Helper.GetActiveCaseID());
				massCopy = new IntegrationPointMassCopy(service);
			}
			catch (Exception ex)
			{
				return new Response
				{
					Success = false,
					Message = "Failed to initialize copying - please contact your administrator",
					Exception = ex
				};
			}

			try
			{
				massCopy.Copy(BatchIDs);
			}
			catch (Exception ex)
			{
				return new Response
				{
					Success = false,
					Message = "Failed to copy one or more Integration Points - please contact your administrator",
					Exception = ex
				};
			}

			return new Response {Success = true};
		}

		#region Unused interface implementation

		public override Response PreMassOperation()
		{
			return new Response {Success = true};
		}

		public override Response PostMassOperation()
		{
			return new Response {Success = true};
		}

		public override Response ValidateSelection()
		{
			return new Response
			{
				Success = true,
				Message = "Warning: The below details the items that will be copied and settings that will be carried over when performing a copy."
			};
		}

		public override Response ValidateLayout()
		{
			return new Response {Success = true};
		}

		#endregion
	}
}