namespace Relativity.Sync
{
	internal sealed class CorrelationId
	{
		public CorrelationId(string value)
		{
			Value = value;
		}

		public string Value { get; }

		public override string ToString()
		{
			return Value;
		}
	}
}