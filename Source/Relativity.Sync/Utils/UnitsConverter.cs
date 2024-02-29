namespace Relativity.Sync.Utils
{
    internal static class UnitsConverter
    {
        public static long MegabyteToBytes(long megabytes)
        {
            return megabytes * 1024 * 1024;
        }

        public static double BytesToMegabytes(long bytes)
        {
            return bytes / 1024.0 / 1024.0;
        }

        public static double BytesToGigabytes(long bytes)
        {
            return bytes / 1024.0 / 1024.0 / 1024.0;
        }
    }
}
