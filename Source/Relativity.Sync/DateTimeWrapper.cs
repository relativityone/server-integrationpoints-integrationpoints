using System;

namespace Relativity.Sync
{
	internal sealed class DateTimeWrapper : IDateTime
	{
		public DateTime Now => DateTime.Now;

		public DateTime UtcNow => DateTime.UtcNow;
	}
}