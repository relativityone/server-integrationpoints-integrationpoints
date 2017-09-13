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

		public int Create(T integrationPoint)
		{
			CheckObject(integrationPoint);
			return Create(new List<T> {integrationPoint}).First();
		}

		public virtual List<int> Create(IEnumerable<T> integrationPoints)
		{
			List<T> localList = integrationPoints.ToList();
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

		public T Read(int artifactId)
		{
			if (artifactId == 0)
			{
				throw new ArgumentException("artifactID");
			}
			return Read(new List<int> {artifactId}).First();
		}

		public virtual List<T> Read(IEnumerable<int> artifactIds)
		{
			List<int> local = artifactIds.ToList();
			if (!local.Any())
			{
				return new List<T>();
			}

			ResultSet<RDO> result;
			using (var rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(_executionIdentity))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				result = rsapiClient.Repositories.RDO.Read(local.ToArray());
			}

			CheckResult(result);

			return result.Results.Select(x => new T {Rdo = x.Artifact}).ToList();
		}

		public bool Update(T obj)
		{
			CheckObject(obj);
			return Update(new List<T> {obj});
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

		public bool Delete(int artifactId)
		{
			if (artifactId == 0)
			{
				throw new ArgumentException("artifactID");
			}
			return Delete(new List<int> {artifactId});
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

		public bool Delete(T obj)
		{
			CheckObject(obj);
			return Delete(new List<int> {obj.ArtifactId});
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

		public virtual List<T> Query(Query<RDO> q, int pageSize = 0)
		{
			if (!q.ArtifactTypeGuid.HasValue)
			{
				q.ArtifactTypeGuid = Guid.Parse(BaseRdo.GetObjectMetadata(typeof(T)).ArtifactTypeGuid);
			}

			QueryResultSet<RDO> result;
			using (var rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(_executionIdentity))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
				result = rsapiClient.Repositories.RDO.Query(q, pageSize);
			}

			CheckResult(result);

			return result.Results.Select(x => new T {Rdo = x.Artifact}).ToList();
		}
	}
}