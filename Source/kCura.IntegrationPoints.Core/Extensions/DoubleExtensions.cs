using System;

namespace kCura.IntegrationPoints.Core.Extensions
{
    public static class DoubleExtensions
    {
        public static double ConvertBytesToGigabytes(this double bytes, int precision = 2)
        {
            return Math.Round(bytes / Math.Pow(1024, 3), precision);
        }
    }
}
