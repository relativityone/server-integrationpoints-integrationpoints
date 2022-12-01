using System;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace Relativity.Sync
{
    /// <summary>
    ///     Provides access to Relativity Services
    /// </summary>
    public interface IRelativityServices
    {
        /// <summary>
        ///     Provides access to Relativity Telemetry
        /// </summary>
        IAPM APM { get; }

        /// <summary>
        ///     Relativity authentication endpoint address
        /// </summary>
        Uri AuthenticationUri { get; }

        /// <summary>
        ///     Interface with helper methods to programmatically interact with Relativity
        /// </summary>
        IHelper Helper { get; }

        /// <summary>
        ///     Relativity EDDS DbContext
        /// </summary>
        IDBContext GetEddsDbContext();
    }
}
