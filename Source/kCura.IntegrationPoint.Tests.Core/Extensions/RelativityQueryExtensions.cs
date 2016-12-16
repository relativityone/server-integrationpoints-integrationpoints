using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
	public static class RelativityQueryExtensions
	{
		public static bool IsEqualOnTypeAndNameAndFields(this Query<RDO> query1, Query<RDO> query2)
		{
			return (query1.ArtifactTypeGuid == query2.ArtifactTypeGuid)
					&& (query1.ArtifactTypeID == query2.ArtifactTypeID)
					&& (query1.ArtifactTypeName == query2.ArtifactTypeName)
					&& query1.Fields.IsEqualTo(query2.Fields);
		}

		private static bool IsEqualTo(this List<FieldValue> fieldValues1, List<FieldValue> fieldValues2)
		{
			if ((fieldValues1 == null) && (fieldValues2 == null))
			{
				return true;
			}
			if ((fieldValues1 == null) || (fieldValues2 == null))
			{
				return false;
			}
			if (fieldValues1 == fieldValues2)
			{
				return true;
			}
			if (fieldValues1.Count != fieldValues2.Count)
			{
				return false;
			}
			return fieldValues1.All(x => fieldValues2.Any(y => (x.Name == y.Name) && (x.ArtifactID == y.ArtifactID) && MatchGuids(x, y)));
		}

		private static bool MatchGuids(FieldValue fieldValue1, FieldValue fieldValue2)
		{
			return fieldValue1.Guids.All(x => fieldValue2.Guids.Any(y => x == y)) && (fieldValue1.Guids.Count == fieldValue2.Guids.Count);
		}
	}
}