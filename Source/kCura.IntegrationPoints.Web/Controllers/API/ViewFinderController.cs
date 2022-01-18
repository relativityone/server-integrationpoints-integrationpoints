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
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactType = Relativity.ArtifactType;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ViewFinderController : ApiController
	{
		private readonly ICPHelper _helper;

		public ViewFinderController(ICPHelper helper)
		{
			_helper = helper;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve view list.")]
		public async Task<HttpResponseMessage> GetViews(int workspaceId, int rdoArtifactTypeId)
		{
			using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
			{
				string rdoName = await GetRdoNameAsync(workspaceId, rdoArtifactTypeId).ConfigureAwait(false);

				QueryRequest request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.View
					},
					Condition = $"'Object Type' == '{rdoName}'",
					IncludeNameInQueryResult = true
				};

				QueryResult queryResult = await objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue).ConfigureAwait(false);

				List<ViewModel> views =  queryResult.Objects.Select(x => new ViewModel(x)).ToList();

				var response = new ViewResultsModel
				{
					Results = views,
					TotalResults = views.Count,
					HasMoreResults = false
				};

				return Request.CreateResponse(HttpStatusCode.OK, response);
			}
		}

		private async Task<string> GetRdoNameAsync(int workspaceId, int rdoArtifactTypeId)
		{
			using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
			{
				QueryRequest request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.ObjectType
					},
					Condition = $"'Artifact Type ID' == {rdoArtifactTypeId}",
					IncludeNameInQueryResult = true
				};

				QueryResult queryResult = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

				if (queryResult.Objects.Any())
				{
					return queryResult.Objects.First().Name;
				}
				else
				{
					throw new InvalidOperationException($"Cannot find Artifact Type ID: {rdoArtifactTypeId}");
				}
			}
		}
	}
}
