using System;

namespace Relativity.Sync
{
    internal sealed class SyncItemLevelErrorException : SyncException
    {
        public SyncItemLevelErrorException()
        {
        }

        public SyncItemLevelErrorException(string message) : base(message)
        {
        }

        public SyncItemLevelErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}