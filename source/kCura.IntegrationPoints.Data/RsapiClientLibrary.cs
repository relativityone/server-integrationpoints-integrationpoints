using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Attributes;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;

namespace kCura.IntegrationPoints.Data
{
	public class RsapiClientLibrary<T> : IGenericLibrary<T> where T: BaseRdo, new()
	{
		private readonly RDORepository _client;
		
		private DynamicObjectAttribute ObjectMetadata { get; set; }
		private Dictionary<Guid, DynamicFieldAttribute> FieldMetadata;
		
		public RsapiClientLibrary(IRSAPIClient client)
		{
			_client = client.Repositories.RDO;
			ObjectMetadata = BaseRdo.GetObjectMetadata(typeof(T));
			FieldMetadata = BaseRdo.GetFieldMetadata(typeof(T));
		}

		private void CheckResult<TResult>(ResultSet<TResult> result) where TResult : kCura.Relativity.Client.DTOs.Artifact
		{
			throw new NotImplementedException();
		}

		public int Create(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			return Create(new List<T>{ obj}).First();
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
			return Read(new List<int> {artifactId}).First();
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
			return readResult.Results.Select(result => new T {Rdo = result.Artifact}).ToList();
		}

		public bool Update(T obj)
		{
			throw new NotImplementedException();
		}

		public bool Update(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
		}

		public bool Delete(int artifactId)
		{
			throw new NotImplementedException();
		}

		public bool Delete(IEnumerable<int> artifactIds)
		{
			throw new NotImplementedException();
		}

		public bool Delete(T obj)
		{
			throw new NotImplementedException();
		}

		public bool Delete(IEnumerable<T> objs)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}
	}
}
