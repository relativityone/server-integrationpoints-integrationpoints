using System;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class DateTimeUtcHelper : IDateTimeHelper
    {
        public DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}