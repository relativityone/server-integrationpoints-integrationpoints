using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Data.Transformers
{
	public static class RDOFieldsConverter
	{
		public static IEnumerable<Guid> GetFieldGuids<T>()
		{
			return typeof(T)
				.GetFields()
				.Select(f => f.GetRawConstantValue())
				.OfType<string>()
				.Select(s => new Guid(s));
		}
	}
}
