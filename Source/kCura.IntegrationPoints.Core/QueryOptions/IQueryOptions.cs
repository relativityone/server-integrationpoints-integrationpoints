using System;

namespace kCura.IntegrationPoints.Core.QueryOptions
{
	public interface IQueryOptions
	{
		Guid[] FieldGuids { get; }
	}
}
