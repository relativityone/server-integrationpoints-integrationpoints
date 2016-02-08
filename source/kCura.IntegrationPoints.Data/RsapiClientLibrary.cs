using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Attributes;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using CollectionExtensions = Castle.Core.Internal.CollectionExtensions;

namespace kCura.IntegrationPoints.Data
{
	public class RsapiClientLibrary<T> : IGenericLibrary<T> where T : BaseRdo, new()
	{
		private readonly RDORepository _client;

		public RsapiClientLibrary(IRSAPIClient client)
		{
			_client = client.Repositories.RDO;
		}

		private static void CheckResult<TResult>(ResultSet<TResult> result) where TResult : kCura.Relativity.Client.DTOs.Artifact
		{
			RdoHelper.CheckResult(result);
		}

		private static void CheckObject(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
		}

		public int Create(T obj)
		{
			CheckObject(obj);
			return Create(new List<T> { obj }).First();
		}

		public List<int> Create(IEnumerable<T> objs)
		{
			var localList = objs.ToList();
			if (!localList.Any())
			{
				return new List<int>();
			}
			var result = _client.Create(localList.Select(x => x.Rdo).ToList());
			CheckResult(result);

			return result.Results.Select(x => x.Artifact.ArtifactID).ToList();
		}

		public T Read(int artifactId)
		{
			if (artifactId == 0)
			{
				throw new ArgumentException("artifactID");
			}
			return Read(new List<int> { artifactId }).First();
		}

		public List<T> Read(IEnumerable<int> artifactIds)
		{
			var local = artifactIds.ToList();
			if (!local.Any())
			{
				return new List<T>();
			}
			var readResult = _client.Read(local.ToArray());
			CheckResult(readResult);
			return readResult.Results.Select(result => new T { Rdo = result.Artifact }).ToList();
		}

		public bool Update(T obj)
		{
			CheckObject(obj);
			return Update(new List<T> { obj });
		}

		public bool Update(IEnumerable<T> objs)
		{
			var localList = objs.ToList();
			if (!localList.Any())
			{
				return true;
			}
			var result = _client.Update(localList.Select(x => x.Rdo).ToList());
			CheckResult(result);
			return result.Success;
		}

		public bool Delete(int artifactId)
		{
			if (artifactId == 0)
			{
				throw new ArgumentException("artifactID");
			}
			return Delete(new List<int> { artifactId });
		}

		public bool Delete(IEnumerable<int> artifactIds)
		{
			var localList = artifactIds.ToList();
			if (!localList.Any())
			{
				return true;
			}
			var result = _client.Delete(localList);
			CheckResult(result);
			return result.Success;
		}

		public bool Delete(T obj)
		{
			CheckObject(obj);
			return Delete(new List<int> { obj.ArtifactId });
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

		public List<T> Query(Query<RDO> q, int pageSize = 0)
		{
			if (!q.ArtifactTypeGuid.HasValue)
			{
				q.ArtifactTypeGuid = Guid.Parse(BaseRdo.GetObjectMetadata(typeof(T)).ArtifactTypeGuid);
			}
			var result = _client.Query(q, pageSize);

			CheckResult(result);
			return result.Results.Select(x => new T { Rdo = x.Artifact }).ToList();
		}
	}
}
