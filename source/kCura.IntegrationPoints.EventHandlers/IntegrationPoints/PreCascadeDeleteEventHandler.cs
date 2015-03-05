using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.Repositories;
using kCura.ScheduleQueue.Core.Data.Queries;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class PreCascadeDeleteEventHandler : kCura.EventHandler.PreCascadeDeleteEventHandler
	{
		private DeleteHistoryService deleteHistoryService;
		private DeleteHistoryErrorService deleteHistoryErrorService;
		public DeleteHistoryErrorService DeleteHistoryErrorService
		{
			get
			{
				return deleteHistoryErrorService ??
							 (deleteHistoryErrorService =
								 new DeleteHistoryErrorService(
									 ServiceContextFactory.CreateRSAPIService(
										 new kCura.IntegrationPoints.Core.RsapiClientFactory(base.Helper).CreateClientForWorkspace(
											 base.Application.ArtifactID))));
			}
			set { deleteHistoryErrorService = value; }
		}
		
		public DeleteHistoryService DeleteHistoryService
		{
			get
			{
				return deleteHistoryService ??
				       (deleteHistoryService =
					       new DeleteHistoryService(
						       ServiceContextFactory.CreateRSAPIService(
							       new kCura.IntegrationPoints.Core.RsapiClientFactory(base.Helper).CreateClientForWorkspace(
								       base.Application.ArtifactID)),DeleteHistoryErrorService));
			}
			set { deleteHistoryService = value; }
		}

		private IWorkspaceDBContext _workspaceDbContext;

		public IWorkspaceDBContext GetWorkspaceDbContext()
		{
			return _workspaceDbContext ??
			       (_workspaceDbContext = new WorkspaceContext(base.Helper.GetDBContext(base.Helper.GetActiveCaseID())));
		}

		public override void Commit()
		{
		}

		public override void Rollback()
		{
		}

		public override Response Execute()
		{
			Response eventResponse = new Response();
			try
			{
				//Get a data reader used to read the ArtifactIDs marked for deletion.
				using (DbDataReader reader = GetArtifactsToBeDeleted(GetWorkspaceDbContext()))
				{
					while (reader.Read())
					{
						//Retrieve the current artifactID from the reader's first (and only) column.
						int artifactID = reader.GetInt32(0);
						
						DeleteHistoryService.DeleteHistoriesAssociatedWithIP(artifactID);
					}
				}

				//Event completed without error.  Return success.

				eventResponse.Success = true;
				eventResponse.Message = "Success";
			}
			catch (Exception ex)
			{
				//Event completed with error. Return failure and mention of the error details.
				eventResponse.Success = false;
				eventResponse.Message = string.Format(
					"An error occurred while executing the Pre-Mass-Event handler.  Message: {0}", ex.Message);
			}

			return eventResponse;
		}

		private DbDataReader GetArtifactsToBeDeleted(IWorkspaceDBContext workspaceContext)
		{
			//Create a sql statement which will select the list of ArtifactIDs
			//from the TempTableNameWithParentArtifactsToDelete scratch table.
			string sql = string.Format("SELECT ArtifactID FROM [EDDSResource]..[{0}]",
				this.TempTableNameWithParentArtifactsToDelete);

			return workspaceContext.ExecuteSqlStatementAsDbDataReader(sql);
		}

		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}
	}
}
