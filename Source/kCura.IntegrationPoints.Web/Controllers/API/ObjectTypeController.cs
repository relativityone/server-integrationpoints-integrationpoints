using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity;
using Relativity.API;
using Relativity.Query;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactType = Relativity.ArtifactType;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ObjectTypeController : ApiController
	{
		private const int _DEFAULT_PAGE_SIZE = 100;
		private readonly ICPHelper _helper;
		private readonly IAPILog _logger;

		public ObjectTypeController(ICPHelper helper)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ObjectTypeController>();
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve view list.")]
		public async Task<HttpResponseMessage> GetViews(int workspaceId, int artifactTypeId, string search = "", int page = 1, int pageSize = _DEFAULT_PAGE_SIZE)
		{
			_logger.LogInformation("Retrieving views for Artifact Type ID {artifactTypeId} in workspace {workspaceId}", artifactTypeId, workspaceId);
			
			try
			{
				using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
				{
					string rdoName = await GetObjectTypeName(workspaceId, artifactTypeId).ConfigureAwait(false);

					QueryRequest request = new QueryRequest()
					{
						ObjectType = new ObjectTypeRef()
						{
							ArtifactTypeID = (int)ArtifactType.View
						},
						Condition = $"'Object Type' == '{rdoName}' AND 'Name' LIKE '%{search}%'",
						IncludeNameInQueryResult = true
					};

					QueryResult queryResult = await objectManager.QueryAsync(workspaceId, request, (page - 1) * pageSize, pageSize).ConfigureAwait(false);

					List<ViewModel> views = queryResult.Objects.Select(x => new ViewModel(x)).ToList();

					var response = new ViewResultsModel
					{
						Results = views,
						TotalResults = queryResult.TotalCount,
						HasMoreResults = page * pageSize < queryResult.TotalCount
					};

					return Request.CreateResponse(HttpStatusCode.OK, response);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving views.");
				throw;
			}
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to check if Object Type exists in workspace.")]
		public async Task<bool> ObjectTypeExists(int sourceWorkspaceId, int destinationWorkspaceId, int sourceObjectTypeArtifactId)
		{
			string objectTypeName = await GetObjectTypeName(sourceWorkspaceId, sourceObjectTypeArtifactId).ConfigureAwait(false);

			_logger.LogInformation("Checking if Object Type {objectTypeName} exists in destination workspae ID {destinationWorkspaceId}",
				objectTypeName, destinationWorkspaceId);
			try
			{
				using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
				{
					QueryRequest request = new QueryRequest()
					{
						ObjectType = new ObjectTypeRef()
						{
							ArtifactTypeID = (int)ArtifactType.ObjectType
						},
						Condition = $"'Name' == '{objectTypeName}'"
					};

					QueryResult queryResult = await objectManager.QueryAsync(destinationWorkspaceId, request, 0, 1).ConfigureAwait(false);

					return queryResult.Objects.Any();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while checking if Object Type exists in destination workspace");
				throw;
			}
		}

		private async Task<string> GetObjectTypeName(int workspaceId, int artifactTypeId)
		{
			_logger.LogInformation("Retrieving Object Type name for Object Type Artifact ID: {artifactTypeId} in workspace {workspaceId}",
				artifactTypeId, workspaceId);

			try
			{
				using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
				{
					QueryRequest request = new QueryRequest()
					{
						ObjectType = new ObjectTypeRef()
						{
							ArtifactTypeID = (int)ArtifactType.ObjectType
						},
						Condition = $"'Artifact Type ID' == {artifactTypeId}",
						IncludeNameInQueryResult = true
					};

					QueryResult queryResult = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

					if (queryResult.Objects.Any())
					{
						return queryResult.Objects.First().Name;
					}
					else
					{
						throw new InvalidOperationException($"Cannot find Artifact Type ID: {artifactTypeId}");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving Object Type name.");
				throw;
			}
		}

	}
}
