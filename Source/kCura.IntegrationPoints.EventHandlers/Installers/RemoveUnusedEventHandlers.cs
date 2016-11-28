using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.Relativity.Client;
using Relativity.API;
using ArtifactType = Relativity.ArtifactType;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Deletes/Removes old event handlers for Integration Point - PreLoad and PageInteraction")]
	[RunOnce(false)]
	[Guid("1A4C2EC1-079E-4F8F-8FC8-1358627617DE")]
	public class RemoveUnusedEventHandlers : PostInstallEventHandler
	{
		private const string _PRE_LAOD_EVENT_HANDLER_GUID = "c77369d2-3f9a-4598-b7bc-229050b3bbe6";
		private const string _PAGE_INTERACTION_EVENT_HANDLER_GUID = "eed5ad4a-3137-4a93-a2b6-3d96e3894cd2";

		private const int _ARTIFACTTYPEID_EVENTHANDLER = (int) ArtifactType.EventHandler;

		public override Response Execute()
		{
			var response = new Response
			{
				Success = true,
				Message = "Old EventHandlers successfully removed."
			};

			List<int> artifactIds = RetrieveArtifactIdsByGuids(new List<string> {_PRE_LAOD_EVENT_HANDLER_GUID, _PAGE_INTERACTION_EVENT_HANDLER_GUID});
			
			if (artifactIds.Count > 0)
			{
				UnlinkFromApplication(artifactIds);

				ResultSet deleteResults = DeleteByArtifactIds(artifactIds);
				response.Success = deleteResults.Success;
				if (!response.Success)
				{
					response.Message = deleteResults.Message;
				}
			}

			return response;
		}

		private void UnlinkFromApplication(List<int> artifactIds)
		{
			try
			{
				string sqlStatement = @"DELETE FROM ApplicationEventHandler
								WHERE EventHandlerArtifactID IN 
									({0})
								AND ApplicationArtifactID =
									(SELECT ArtifactID
									FROM ArtifactGuid
									WHERE ArtifactGuid = @ApplicationGuid)";

				var ids = string.Join(",", artifactIds);
				SqlParameter applicationGuidParameter = new SqlParameter("@ApplicationGuid", Constants.IntegrationPoints.APPLICATION_GUID_STRING);

				IDBContext context = Helper.GetDBContext(Helper.GetActiveCaseID());
				context.ExecuteNonQuerySQLStatement(string.Format(sqlStatement, ids), new[] {applicationGuidParameter});
			}
			catch (Exception e)
			{
				Helper.GetLoggerFactory().GetLogger().ForContext<RemoveUnusedEventHandlers>().LogError(e, "Failed to unlink event handlers from application");
			}
		}

		private List<int> RetrieveArtifactIdsByGuids(IList<string> guids)
		{
			IDBContext context = Helper.GetDBContext(Helper.GetActiveCaseID());
			var result = new List<int>();
			foreach (var guid in guids)
			{
				int artifactId = context.ExecuteSqlStatementAsScalar<int>("Select ArtifactID from ArtifactGuid where artifactGuid = @artifactGuid",
					new SqlParameter("@artifactGuid", guid));
				if (artifactId > 0)
				{
					result.Add(artifactId);
				}
			}
			return result;
		}

		private ResultSet DeleteByArtifactIds(List<int> artifactIds)
		{
			using (IRSAPIClient client = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				client.APIOptions.WorkspaceID = Helper.GetActiveCaseID();

				MassDeleteOptions options = new MassDeleteOptions(_ARTIFACTTYPEID_EVENTHANDLER) {CascadeDelete = true};

				ResultSet deleteResults = client.MassDelete(client.APIOptions, options, artifactIds);

				return deleteResults;
			}
		}
	}
}