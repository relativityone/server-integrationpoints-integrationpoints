using System;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiRdoRepository : IRdoRepository
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IAPILog _logger;
		private readonly int _workspaceArtifactId;

		public RsapiRdoRepository(IHelper helper, IServicesMgr servicesMgr, int workspaceArtifactId)
		{
			_servicesMgr = servicesMgr;
			_workspaceArtifactId = workspaceArtifactId;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RsapiRdoRepository>();
		}

		public QueryResultSet<RDO> Query(Query<RDO> query)
		{
			using (IRSAPIClient rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					var result = rsapiClient.Repositories.RDO.Query(query);
					if (!result.Success)
					{
						var messages = result.Results.Where(x => !x.Success).Select(x => x.Message);
						throw new AggregateException(result.Message, messages.Select(x => new Exception(x)));
					}
					return result;
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Failed to query RDOs.");
					throw new Exception($"Unable to retrieve RDO: {e.Message}", e);
				}
			}
		}

		public RDO QuerySingle(Query<RDO> query)
		{
			var queryResult = Query(query).Results;
			if (queryResult.Count == 0)
			{
				throw new Exception("Unable to retrieve RDO.");
			}
			return queryResult[0].Artifact;
		}

		public int Create(RDO rdo)
		{
			try
			{
				using (var rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					return rsapiClient.Repositories.RDO.CreateSingle(rdo);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to create RDO.");
				throw;
			}
		}

		public void Update(RDO rdo)
		{
			try
			{
				using (var rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					rsapiClient.Repositories.RDO.UpdateSingle(rdo);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to update RDO.");
				throw;
			}
		}

		public RDO ReadSingle(int artifactId)
		{
			try
			{
				using (var rsapiClient = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					return rsapiClient.Repositories.RDO.ReadSingle(artifactId);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to read RDO.");
				throw;
			}
		}
	}
}