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

		public static List<T> ReadAll<T>(this IGenericLibrary<T> service, params Guid[] guids) where T : BaseRdo, new()
		{
			var fields = new List<FieldValue>(guids.Select(x => new FieldValue(x)));
			return service.ReadAll(fields);
		}

		public static List<T> ReadAll<T>(this IGenericLibrary<T> service) where T : BaseRdo, new()
		{
			return service.ReadAll(GetAllFields<T>());
		}

		private static List<T> ReadAll<T>(this IGenericLibrary<T> service, List<FieldValue> fields) where T : BaseRdo, new()
		{
			var query = new Query<RDO>
			{
				ArtifactTypeGuid = Guid.Parse(BaseRdo.GetObjectMetadata(typeof(T)).ArtifactTypeGuid),
				Fields = fields,
				Condition = new ObjectCondition("ArtifactID", ObjectConditionEnum.IsSet)
			};
			
			return service.Query(query);
		}

		private static List<FieldValue> GetAllFields<T>()
		{
			return (from field in BaseRdo.GetFieldMetadata(typeof(T)).Values.ToList()
					select new FieldValue(field.FieldGuid)).ToList();
		}
	}
}
