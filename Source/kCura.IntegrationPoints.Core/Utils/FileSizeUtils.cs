using System;

namespace kCura.IntegrationPoints.Core.Utils
{
    public static class FileSizeUtils
    {
        public static string FormatFileSize(long? bytes)
        {
            if (!bytes.HasValue || bytes == 0)
            {
                return "0 Bytes";
            }

            var k = 1024L;
            string[] sizes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            var i = (long)Math.Floor(Math.Log((long)bytes) / Math.Log(k));
            return $"{bytes / Math.Pow(k, i):0.##} {sizes[i]}";
        }
    }
}
