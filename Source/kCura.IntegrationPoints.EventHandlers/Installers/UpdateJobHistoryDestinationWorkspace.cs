using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Update Job History with Relativity Instance information")]
	[RunOnce(true)]
	[Guid("72BDF6BC-A222-4D53-B470-F9A521F22121")]
	public class UpdateJobHistoryDestinationWorkspace : PostInstallEventHandler
	{
		private IJobHistoryService _jobHistoryService;
		private IDestinationParser _destinationParser;

		public UpdateJobHistoryDestinationWorkspace()
		{
		}

		internal UpdateJobHistoryDestinationWorkspace(IJobHistoryService jobHistoryService,
			IDestinationParser destinationParser)
		{
			_jobHistoryService = jobHistoryService;
			_destinationParser = destinationParser;
		}

		public override Response Execute()
		{
			ResolveDependencies();
			return ExecuteInternal();
		}

		internal Response ExecuteInternal()
		{
			try
			{
				IList<Data.JobHistory> jobHistories = _jobHistoryService.GetAll();

				foreach (Data.JobHistory jobHistory in jobHistories)
				{
					string[] elements = _destinationParser.GetElements(jobHistory.DestinationWorkspace);

					if (elements.Length == 2)
					{
						jobHistory.DestinationWorkspace = FederatedInstanceManager.LocalInstance.Name + " - " + jobHistory.DestinationWorkspace;
						_jobHistoryService.UpdateRdo(jobHistory);
					}
				}
			}
			catch (Exception e)
			{
				var response = new Response
				{
					Message = $"Updating Job History/Destination Workspace failed. Exception message: {e.Message}.",
					Exception = e,
					Success = false
				};
				return response;
			}

			return new Response
			{
				Message = "Updated successfully.",
				Success = true
			};
		}

		private void ResolveDependencies()
		{
			_jobHistoryService = CreateJobHistoryService();
			_destinationParser = new DestinationParser();
		}

		private IJobHistoryService CreateJobHistoryService()
		{
			var caseContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
			IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
			var federatedInstanceManager = new FederatedInstanceManager(repositoryFactory, new AlwaysDisabledToggleProvider());
			IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
			ISerializer serializer = new JSONSerializer();
			return new JobHistoryService(caseContext, federatedInstanceManager, workspaceManager, Helper, serializer);
		}
	}
}
