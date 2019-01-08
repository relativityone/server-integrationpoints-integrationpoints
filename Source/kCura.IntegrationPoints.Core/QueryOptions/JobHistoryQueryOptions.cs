using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;

namespace kCura.IntegrationPoints.Core.QueryOptions
{
	public class JobHistoryQueryOptions
	{
		public string[] Fields { get; }

		private JobHistoryQueryOptions(string[] fields)
		{
			Fields = fields;
		}

		public static JobHistoryQueryOptions Query => new JobHistoryQueryOptions(fields: null);

		public JobHistoryQueryOptions All()
		{
			string[] fields = RDOFieldsConverter
				.GetFieldNames<JobHistoryFields>()
				.ToArray();
			return new JobHistoryQueryOptions(fields);
		}

		public JobHistoryQueryOptions Except(string field)
		{
			string[] fields = Fields.Except(new[] { field }).ToArray();
			return new JobHistoryQueryOptions(fields);
		}
	}
}
