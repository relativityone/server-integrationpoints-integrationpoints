using System;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;

namespace kCura.IntegrationPoints.Core.QueryOptions
{
	public class JobHistoryQueryOptions
	{
		public Guid[] FieldGuids { get; }

		private JobHistoryQueryOptions(Guid[] fieldGuids)
		{
			FieldGuids = fieldGuids;
		}

		public static JobHistoryQueryOptions All()
		{
			Guid[] fields = RDOFieldsConverter
				.GetFieldGuids<JobHistoryFieldGuids>()
				.ToArray();
			return new JobHistoryQueryOptions(fields);
		}

		public JobHistoryQueryOptions Except(string fieldGuidAsString)
		{
			var fieldGuid = new Guid(fieldGuidAsString);
			Guid[] fields = FieldGuids.Except(new[] { fieldGuid }).ToArray();
			return new JobHistoryQueryOptions(fields);
		}

		public bool ContainsAll()
		{
			return FieldGuids.SequenceEqual(All().FieldGuids);
		}
	}
}
