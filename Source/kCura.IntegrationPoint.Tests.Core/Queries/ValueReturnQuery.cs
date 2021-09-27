using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoint.Tests.Core.Queries
{
	public class ValueReturnQuery<T> : IQuery<T>
	{
		private readonly T _value;

		public ValueReturnQuery(T value)
		{
			_value = value;
		}

		public T Execute()
		{
			return _value;
		}
	}
}
