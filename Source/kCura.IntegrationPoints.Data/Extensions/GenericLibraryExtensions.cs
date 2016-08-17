using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class GenericLibraryExtensions
	{
		public static T Read<T>(this IGenericLibrary<T> service, int artifactId, IEnumerable<Guid> guids) where T : BaseRdo, new()
		{
			var q = new Query<RDO>();
			q.Fields = new List<FieldValue>(guids.Select(x=>new FieldValue(x)));
			q.ArtifactTypeGuid = Guid.Parse(BaseRdo.GetObjectMetadata(typeof (T)).ArtifactTypeGuid);
			q.Condition = new ObjectCondition( "ArtifactID",ObjectConditionEnum.EqualTo, artifactId);
			return service.Query(q).First();
		}

		public static T Read<T>(this IGenericLibrary<T> service, int artifactId, params Guid[] guids) where T : BaseRdo, new()
		{
			return Read(service, artifactId, guids.ToList());
		}

		public static IEnumerable<T> ReadAll<T>(this IGenericLibrary<T> service, params Guid[] guids) where T : BaseRdo, new()
		{
			var q = new Query<RDO>();
			q.Fields = new List<FieldValue>(guids.Select(x => new FieldValue(x)));
			q.ArtifactTypeGuid = Guid.Parse(BaseRdo.GetObjectMetadata(typeof(T)).ArtifactTypeGuid);
			q.Condition = new ObjectCondition("ArtifactID", ObjectConditionEnum.IsSet);
			return service.Query(q);
		}

	}
}
