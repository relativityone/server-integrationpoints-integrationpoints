using System;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiRdoRepository : IRdoRepository
	{
		private readonly IAPILog _logger;
		private readonly int _workspaceArtifactId;
		private readonly IRsapiClientFactory _rsapiClientFactory;

		public RsapiRdoRepository(IHelper helper, int workspaceArtifactId, IRsapiClientFactory rsapiClientFactory)
		{
			_workspaceArtifactId = workspaceArtifactId;
			_rsapiClientFactory = rsapiClientFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RsapiRdoRepository>();
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
				using (var rsapiClient = GetRsapiClient())
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
				using (var rsapiClient = GetRsapiClient())
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
				using (var rsapiClient = GetRsapiClient())
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

		private IRSAPIClient GetRsapiClient()
		{
			return _rsapiClientFactory.CreateUserClient(_workspaceArtifactId);
		}

		private QueryResultSet<RDO> Query(Query<RDO> query)
		{
			using (IRSAPIClient rsapiClient = GetRsapiClient())
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
	}
}