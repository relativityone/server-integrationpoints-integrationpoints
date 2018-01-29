using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.UtilityDTO
{
	public class RelativityObjectQuery
	{
		public string Condition { get; set; }
		public IEnumerable<RelativityObjectSort> Sorts { get; set; }
		public IEnumerable<RelativityObjectField> Fields { get; set; }
		public RelativityObjectType ObjectType { get; set; }

		public QueryRequest ToObjectManagerQuery<T>()
		{
			return new QueryRequest();
		}
	}
}
