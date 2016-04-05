using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;

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
			FieldEntry[] fields = base.GetFields(options).ToArray();
			foreach (var field in fields.Where(field => field.IsIdentifier))
			{
				field.IsRequired = true;
			}
			return fields;
		}
	}
}