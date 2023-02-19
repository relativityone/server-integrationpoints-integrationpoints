using System;

namespace kCura.IntegrationPoints.Common.Helpers
{
    public class DateTimeWrapper : IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
