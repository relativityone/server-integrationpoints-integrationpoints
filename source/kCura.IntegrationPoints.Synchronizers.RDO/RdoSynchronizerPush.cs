using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	/// <summary>
	/// This exists in the event that the pull behavior differs from the push behavior
	/// </summary>
	public class RdoSynchronizerPush : RdoSynchronizerBase
	{
		public RdoSynchronizerPush(IRelativityFieldQuery fieldQuery, IImportApiFactory factory)
			: base(fieldQuery, factory)
		{
		}

		public override IEnumerable<FieldEntry> GetFields(string options)
		{
			HashSet<string> ignoreFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
				Contracts.Constants.SPECIAL_JOBHISTORY_FIELD_NAME,
				JobHistoryFields.DestinationWorkspace,
				IntegrationPointFields.JobHistory
			};

			FieldEntry[] fields = base.GetFields(options).Where(f => !ignoreFields.Contains(f.ActualName)).Select(f => f).ToArray();

			foreach (var field in fields.Where(field => field.IsIdentifier))
			{
				field.IsRequired = true;
			}
			return fields;
		}
	}
}