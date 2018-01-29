using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Artifact = kCura.Relativity.Client.DTOs.Artifact;

namespace kCura.IntegrationPoints.Data
{
	public class RsapiClientLibrary<T> : IGenericLibrary<T> where T : BaseRdo, new()
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;
		private readonly ExecutionIdentity _executionIdentity;

		public RsapiClientLibrary(IHelper helper, int workspaceArtifactId) : this(helper, workspaceArtifactId, ExecutionIdentity.CurrentUser)
		{
		}

		public RsapiClientLibrary(IHelper helper, int workspaceArtifactId, ExecutionIdentity executionIdentity)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
			_executionIdentity = executionIdentity;
		}

		private static void CheckResult<TResult>(ResultSet<TResult> result) where TResult : Artifact
		{
			result.CheckResult();
		}

		private static void CheckObject(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}
		}

		public virtual List<int> Create(IEnumerable<T> rdos)
		{
			List<T> localList = rdos.ToList();
			if (!localList.Any())
			{
				return new List<int>();
			}

			WriteResultSet<RDO> result;
			using (var rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(_executionIdentity))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				result = rsapiClient.Repositories.RDO.Create(localList.Select(x => x.Rdo).ToList());
			}

			CheckResult(result);

			return result.Results.Select(x => x.Artifact.ArtifactID).ToList();
		}

		public virtual bool Update(IEnumerable<T> objs)
		{
			List<T> localList = objs.ToList();
			if (!localList.Any())
			{
				return true;
			}

			WriteResultSet<RDO> result;
			using (var rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(_executionIdentity))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				result = rsapiClient.Repositories.RDO.Update(localList.Select(x => x.Rdo).ToList());
			}

			CheckResult(result);
			return result.Success;
		}

		public bool Delete(IEnumerable<int> artifactIds)
		{
			List<int> localList = artifactIds.ToList();
			if (!localList.Any())
			{
				return true;
			}

			WriteResultSet<RDO> result;
			using (var rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(_executionIdentity))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				result = rsapiClient.Repositories.RDO.Delete(localList);
			}

			CheckResult(result);
			return result.Success;
		}

		public bool Delete(IEnumerable<T> objs)
		{
			return Delete(objs.Select(x => x.ArtifactId));
		}

		public void MassDelete(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
		}

		public MassCreateResult MassCreate(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
		}

		public MassEditResult MassEdit(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
		}
	}
}