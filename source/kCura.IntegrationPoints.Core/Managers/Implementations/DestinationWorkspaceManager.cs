using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers
{
	public class DestinationWorkspaceManager : IDestinationWorkspaceManager
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private Job _job;
		private readonly ITempDocTableHelper _tempDocHelper;
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;

		// TODO: The IHelper needs to be removed from this constructor -- biedrzycki: April 6th, 2016
		public DestinationWorkspaceManager(IHelper helper, IRepositoryFactory repositoryFactory, Job job, SourceConfiguration sourceConfig, string tableSuffix)
		{
			_repositoryFactory = repositoryFactory;
			_job = job;
			_tempDocHelper = new TempDocumentFactory().GetDocTableHelper(helper, Constants.IntegrationPoints.TEMPORARY_DOCUMENT_TABLE_NAME, tableSuffix, sourceConfig.SourceWorkspaceArtifactId);
			_destinationWorkspaceRepository = _repositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId, sourceConfig.TargetWorkspaceArtifactId);
		}
		public void Execute()
		{
			List<int> documentIds = _tempDocHelper.GetDocumentIdsFromTable(); 
			int documentCount = documentIds.Count;
			int batchSize = DestinationWorkspaceObject.BATCH_SIZE;
			if (documentCount == 0)
			{
				_tempDocHelper.DeleteTable();
				return;
			}

			int destinationWorkspaceRdoId = _destinationWorkspaceRepository.QueryDestinationWorkspaceRdoInstance();
			FieldValueList<Relativity.Client.DTOs.Artifact> existingMultiObjectLinks = null;

			int numberOfBatches = (documentCount + batchSize - 1) / batchSize;
			bool firstUpdateDone = false;
			for (int batchSet = 0; batchSet < numberOfBatches; batchSet++)
			{
				IEnumerable<int> batchedDocIds = documentIds.Skip(batchSet * batchSize).Take(batchSize);
				if (destinationWorkspaceRdoId == -1)
				{
					destinationWorkspaceRdoId = _destinationWorkspaceRepository.CreateDestinationWorkspaceRdoInstance(batchedDocIds.ToList());
				}
				else
				{
					_destinationWorkspaceRepository.UpdateDestinationWorkspaceRdoInstance(batchedDocIds.ToList(), destinationWorkspaceRdoId, ref existingMultiObjectLinks, firstUpdateDone);
					firstUpdateDone = true;
				}
			}

			//todo: link to JobHistoryRDO as well
			_tempDocHelper.DeleteTable();
				
		}
	}
}
