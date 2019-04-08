namespace Relativity.Sync
{
	internal sealed class ProgressStateCounter : IProgressStateCounter
	{
		private int _current;

		public ProgressStateCounter() : this(0) { }

		public ProgressStateCounter(int initial)
		{
			_current = initial;
		}

		public int Next()
		{
			int value = _current;
			_current += 1;
			return value;
		}
	}
}
