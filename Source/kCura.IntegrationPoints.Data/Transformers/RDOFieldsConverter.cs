using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Data.Transformers
{
	public static class RDOFieldsConverter
	{
		public static IEnumerable<string> GetFieldNames<T>() where T : BaseFields
		{
			return typeof(T)
				.GetFields()
				.Select(f => f.GetRawConstantValue())
				.OfType<string>();
		}
	}
}
