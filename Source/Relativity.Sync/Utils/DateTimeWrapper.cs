using System;

namespace Relativity.Sync.Utils
{
    internal sealed class DateTimeWrapper : IDateTime
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
