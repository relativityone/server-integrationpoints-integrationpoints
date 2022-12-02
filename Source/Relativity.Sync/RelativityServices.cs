using System;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace Relativity.Sync
{
    /// <inheritdoc />
    public sealed class RelativityServices : IRelativityServices
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public RelativityServices(IAPM apm, Uri authenticationUri, IHelper helper)
        {
            Helper = helper;
            APM = apm;
            AuthenticationUri = authenticationUri;
        }

        /// <inheritdoc />
        public IAPM APM { get; }

        /// <inheritdoc />
        public IHelper Helper { get; }

        /// <inheritdoc />
        public Uri AuthenticationUri { get; }

        /// <inheritdoc />
        public IDBContext GetEddsDbContext()
        {
            return Helper.GetDBContext(-1);
        }
    }
}
