using System;
using System.Collections.Generic;
using System.Data.Common;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public class PreCascadeDeleteEventHandler : EventHandler.PreCascadeDeleteEventHandler
	{
		private IPreCascadeDeleteEventHandlerValidator _preCascadeDeleteEventHandlerValidator;
		private IRepositoryFactory _repositoryFactory;

		private IWorkspaceDBContext _workspaceDbContext;

		private DeleteHistoryService _deleteHistoryService;

		public PreCascadeDeleteEventHandler()
		{
			//cant be initialized in constractor. IHelper is not initialized at that time and equals null.
			//_repositoryFactory = new RepositoryFactory(this.Helper);
		}

		internal PreCascadeDeleteEventHandler(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		internal IPreCascadeDeleteEventHandlerValidator PreCascadeDeleteEventHandlerValidator
		{
			get
			{
				if (_preCascadeDeleteEventHandlerValidator == null)
				{
					_preCascadeDeleteEventHandlerValidator = new PreCascadeDeleteEventHandlerValidator(new QueueManager(RepositoryFactory), RepositoryFactory);
				}
				return _preCascadeDeleteEventHandlerValidator;
			}
		}

		internal IRepositoryFactory RepositoryFactory
		{
			get
			{
				if (_repositoryFactory == null)
				{
                    _repositoryFactory = new RepositoryFactory(this.Helper, this.Helper.GetServicesManager());
				}
				return _repositoryFactory;
			}
		}

		public DeleteHistoryService DeleteHistoryService
		{
			get
			{
				return _deleteHistoryService ??
						(_deleteHistoryService =
                           new DeleteHistoryService(
								ServiceContextFactory.CreateRSAPIService(Helper, Application.ArtifactID)));
			}
			set { _deleteHistoryService = value; }
		}

		public override FieldCollection RequiredFields => new FieldCollection();

		public IWorkspaceDBContext GetWorkspaceDbContext()
		{
			return _workspaceDbContext ??
					(_workspaceDbContext = new WorkspaceContext(Helper.GetDBContext(Helper.GetActiveCaseID())));
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
				List<int> artifactIds = new List<int>();
				//Get a data reader used to read the ArtifactIDs marked for deletion.
				int wkspId = Helper.GetActiveCaseID();
				using (DbDataReader reader = GetArtifactsToBeDeleted(GetWorkspaceDbContext(), wkspId))
				{
					while (reader.Read())
					{
						//Retrieve the current artifactID from the reader's first (and only) column.
						artifactIds.Add(reader.GetInt32(0));
					}
				}
				artifactIds.ForEach(artifactId => PreCascadeDeleteEventHandlerValidator.Validate(wkspId, artifactId));
				artifactIds.ForEach(artifactId => DeleteHistoryService.DeleteHistoriesAssociatedWithIP(artifactId));

				//Event completed without error.  Return success.
				eventResponse.Success = true;
				eventResponse.Message = "Success";
			}
			catch (Exception ex)
			{
				LogExecutingPreCascadeDeleteError(ex);
				//Event completed with error. Return failure and mention of the error details.
				eventResponse.Success = false;
				eventResponse.Exception = new SystemException(
					$"An error occurred while executing the Mass Delete operation.  Message: {ex.Message}");
			}
			return eventResponse;
		}

		private DbDataReader GetArtifactsToBeDeleted(IWorkspaceDBContext workspaceContext, int workspaceID)
		{
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
			IScratchTableRepository scratchTableRepository = RepositoryFactory.GetScratchTableRepository(workspaceID, string.Empty, string.Empty);
			//Create a sql statement which will select the list of ArtifactIDs
			//from the TempTableNameWithParentArtifactsToDelete scratch table.
			string sql = string.Format("SELECT ArtifactID FROM {0}.[{1}]", scratchTableRepository.GetResourceDBPrepend(), TempTableNameWithParentArtifactsToDelete);

			return workspaceContext.ExecuteSqlStatementAsDbDataReader(sql);
		}

		#region Logging

		private void LogExecutingPreCascadeDeleteError(Exception ex)
		{
			var logger = Helper.GetLoggerFactory().GetLogger().ForContext<PreCascadeDeleteEventHandler>();
			logger.LogError(ex, "An error occurred while executing the Mass Delete operation.");
		}

		#endregion
	}
}
