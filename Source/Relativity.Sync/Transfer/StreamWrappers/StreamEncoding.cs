namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal enum StreamEncoding
    {
        /// <summary>
        /// Indicates that the text being streamed is encoded in ASCII.
        /// </summary>
        ASCII,

        /// <summary>
        /// Indicates that the text being streamed is encoded in Unicode (i.e. little-endian UTF-16).
        /// </summary>
        Unicode
    }
}
