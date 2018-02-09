using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Artifact = kCura.Relativity.Client.DTOs.Artifact;

namespace kCura.IntegrationPoints.Data
{
	public class RsapiClientLibrary<T> : IGenericLibrary<T> where T : BaseRdo, new()
	{
		private readonly IHelper _helper;
		private readonly IAPILog _logger;
		private readonly int _workspaceArtifactId;
		private readonly ExecutionIdentity _executionIdentity;
		private readonly IRsapiClientFactory _rsapiClientFactory;

		public RsapiClientLibrary(IHelper helper, int workspaceArtifactId) : this(helper, workspaceArtifactId, ExecutionIdentity.CurrentUser)
		{
		}

		public RsapiClientLibrary(IHelper helper, int workspaceArtifactId, ExecutionIdentity executionIdentity)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RsapiClientLibrary<T>>();
			_workspaceArtifactId = workspaceArtifactId;
			_executionIdentity = executionIdentity;
			_rsapiClientFactory = new RsapiClientFactory();
		}

		public virtual List<int> Create(IEnumerable<T> rdos)
		{
			List<T> localList = ConvertEnumerableToList(rdos);
			if (!localList.Any())
			{
				return new List<int>();
			}

			WriteResultSet<RDO> result;
			try
			{
				using (var rsapiClient = _rsapiClientFactory.CreateClient(_helper, _executionIdentity))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					result = rsapiClient.Repositories.RDO.Create(localList.Select(x => x.Rdo).ToList());
				}
			}
			catch (Exception e)
			{
				throw CreateRsapiClientException(e, "Create");
			}

			try
			{
				CheckResult(result);
				return result.Results.Select(x => x.Artifact.ArtifactID).ToList();
			}
			catch (Exception e)
			{
				throw CreateRsapiClientPostprocessingException(e, "Create");
			}
		}

		public virtual bool Update(IEnumerable<T> rdos)
		{
			List<T> localList = ConvertEnumerableToList(rdos);
			if (!localList.Any())
			{
				return true;
			}

			WriteResultSet<RDO> result;
			try
			{
				using (var rsapiClient = _rsapiClientFactory.CreateClient(_helper, _executionIdentity))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					result = rsapiClient.Repositories.RDO.Update(localList.Select(x => x.Rdo).ToList());
				}
			}
			catch (Exception e)
			{
				throw CreateRsapiClientException(e, "Update");
			}

			try
			{
				CheckResult(result);
				return result.Success;
			}
			catch (Exception e)
			{
				throw CreateRsapiClientPostprocessingException(e, "Update");
			}
		}

		public bool Delete(IEnumerable<int> artifactIds)
		{
			List<int> localList = ConvertEnumerableToList(artifactIds);
			if (!localList.Any())
			{
				return true;
			}

			WriteResultSet<RDO> result;
			try
			{
				using (var rsapiClient = _rsapiClientFactory.CreateClient(_helper, _executionIdentity))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					result = rsapiClient.Repositories.RDO.Delete(localList);
				}
			}
			catch (Exception e)
			{
				throw CreateRsapiClientException(e, "Delete");
			}

			try
			{
				CheckResult(result);
				return result.Success;
			}
			catch (Exception e)
			{
				throw CreateRsapiClientPostprocessingException(e, "Delete");
			}
		}

		public bool Delete(IEnumerable<T> rdos)
		{
			return Delete(rdos?.Select(x => x.ArtifactId));
		}

		private static string RdoType => typeof(T).FullName;

		private IntegrationPointsException CreateRsapiClientException(Exception e, string operationName)
		{
			_logger.LogError(e, "Cannot {operationName} objects using RsapiClientLibrary. Rdo type: {RdoType}. Workspace: {_workspaceArtifactId}. ExecutionIdentity: {_executionIdentity}",
				operationName, RdoType, _workspaceArtifactId, _executionIdentity);
			string message = $"Cannot {operationName} objects using RsapiClientLibrary. Rdo type: {RdoType}. Workspace: {_workspaceArtifactId}. ExecutionIdentity: {_executionIdentity}";
			return CreateIntegrationPointExceptionForRsapi(e, message);
		}

		private IntegrationPointsException CreateRsapiClientPostprocessingException(Exception e, string operationName)
		{
			string messageTemplate =
				"Exception while processing results of IRSAPIClient request for {operationName} objects. Rdo type: {RdoType}. Workspace: {_workspaceArtifactId}. ExecutionIdentity: {_executionIdentity}";
			_logger.LogError(e, messageTemplate, operationName, RdoType, _workspaceArtifactId, _executionIdentity);
			string message = $"Exception while processing results of IRSAPIClient request for {operationName} objects."
				+ $" Rdo type: {RdoType}. Workspace: {_workspaceArtifactId}. ExecutionIdentity: {_executionIdentity}";
			return CreateIntegrationPointExceptionForRsapi(e, message);
		}

		private IntegrationPointsException CreateIntegrationPointExceptionForRsapi(Exception e, string message)
		{
			return new IntegrationPointsException(message, e)
			{
				ShouldAddToErrorsTab = true,
				ExceptionSource = IntegrationPointsExceptionSource.RSAPI
			};
		}

		private List<TEnum> ConvertEnumerableToList<TEnum>(IEnumerable<TEnum> enumerable)
		{
			return (enumerable ?? Enumerable.Empty<TEnum>()).ToList();
		}

		private static void CheckResult<TResult>(ResultSet<TResult> result) where TResult : Artifact
		{
			result.CheckResult();
		}
	}
}