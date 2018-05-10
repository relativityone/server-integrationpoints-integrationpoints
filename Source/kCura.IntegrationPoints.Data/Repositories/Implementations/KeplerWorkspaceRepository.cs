using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class KeplerWorkspaceRepository : KeplerServiceBase, IWorkspaceRepository
	{
		private readonly IAPILog _logger;
		private readonly IServicesMgr _servicesMgr;

		public KeplerWorkspaceRepository(IHelper helper, IServicesMgr servicesMgr, IRelativityObjectManager relativityObjectManager)
			: base(relativityObjectManager)
		{

			_logger = helper.GetLoggerFactory().GetLogger().ForContext<KeplerWorkspaceRepository>();
			_servicesMgr = servicesMgr;
		}

		public WorkspaceDTO Retrieve(int workspaceArtifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser)
		{
			ArtifactDTO[] workspaces = null;
			var query = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
				Fields = new List<FieldRef>() { new FieldRef() { Name = "Name" } },
				Condition = $"'ArtifactID' == {workspaceArtifactId}",
			};

			try
			{
				workspaces = RetrieveAllArtifactsAsync(query, executionIdentity).GetResultsWithoutContextSync();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to retrieve workspace {WorkspaceArtifactId}", workspaceArtifactId);
				throw;
			}

			return Convert(workspaces).FirstOrDefault();
		}

		public IEnumerable<WorkspaceDTO> RetrieveAll()
		{
			var query = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Case },
				Fields = new List<FieldRef>() { new FieldRef() { Name = "Name" } }
			};

			ArtifactDTO[] artifactDtos;
			try
			{
				artifactDtos = RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to retrieve workspaces");
				throw;
			}

			return Convert(artifactDtos);
		}

		public IEnumerable<WorkspaceDTO> RetrieveAllActive()
		{
			IEnumerable<WorkspaceDTO> activeWorkspaces;

			using (IWorkspaceManager workspaceManagerProxy =
				_servicesMgr.CreateProxy<IWorkspaceManager>(ExecutionIdentity.CurrentUser))
			{
				IEnumerable<WorkspaceRef> result = workspaceManagerProxy.RetrieveAllActive().Result;
				activeWorkspaces = result.Select(x => new WorkspaceDTO()
				{
					Name = x.Name,
					ArtifactId = x.ArtifactID
				});
			}

			return activeWorkspaces;
		}

		private IEnumerable<WorkspaceDTO> Convert(IEnumerable<ArtifactDTO> artifactDtos)
		{
			var workspaces = new List<WorkspaceDTO>();

			foreach (ArtifactDTO artifactDto in artifactDtos)
			{
				workspaces.Add(new WorkspaceDTO()
				{
					ArtifactId = artifactDto.ArtifactId,
					Name = (string)artifactDto.Fields[0].Value
				});
			}

			return workspaces;
		}

	}
}