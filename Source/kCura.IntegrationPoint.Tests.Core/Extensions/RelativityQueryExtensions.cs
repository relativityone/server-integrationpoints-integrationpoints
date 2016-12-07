using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
	public static class RelativityQueryExtensions
	{
		public static bool IsEqualOnTypeAndNameAndFields(this Query<RDO> q1, Query<RDO> q2)
		{
			return (q1.ArtifactTypeGuid == q2.ArtifactTypeGuid)
					&& (q1.ArtifactTypeID == q2.ArtifactTypeID)
					&& (q1.ArtifactTypeName == q2.ArtifactTypeName)
					&& q1.Fields.IsEqualTo(q2.Fields);
		}

		private static bool IsEqualTo(this List<FieldValue> f1, List<FieldValue> f2)
		{
			if ((f1 == null) && (f2 == null))
			{
				return true;
			}
			if ((f1 == null) || (f2 == null))
			{
				return false;
			}
			if (f1 == f2)
			{
				return true;
			}
			if (f1.Count != f2.Count)
			{
				return false;
			}
			return f1.All(x => f2.Any(y => (x.Name == y.Name) && (x.ArtifactID == y.ArtifactID) && MatchGuids(x, y)));
		}

		private static bool MatchGuids(FieldValue f1, FieldValue f2)
		{
			return f1.Guids.All(x => f2.Guids.Any(y => x == y)) && (f1.Guids.Count == f2.Guids.Count);
		}
	}
}