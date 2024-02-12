using System;

namespace Relativity.Sync.Utils
{
    /// <summary>
    /// Interface for <see cref="System.DateTime"/>
    /// </summary>
    public interface IDateTime
    {
        /// <inheritdoc cref="System.DateTime"/>
        DateTime Now { get; }

        /// <inheritdoc cref="System.DateTime"/>
        DateTime UtcNow { get; }
    }
}
