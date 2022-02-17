using System;
using Relativity.API;
using Relativity.Sync.KeplerFactory;
using Relativity.Telemetry.APM;

namespace Relativity.Sync
{
	/// <inheritdoc />
	public sealed class RelativityServices : IRelativityServices
	{
		/// <summary>
		///     Constructor
		/// </summary>
		public RelativityServices(IAPM apm, ISyncServiceManager servicesMgr, ISourceServiceFactoryForAdmin servicesMgrForAdmin , Uri authenticationUri, IHelper helper)
		{
			Helper = helper;
			APM = apm;
            ServicesMgr = servicesMgr;
            ServicesMgrForAdmin = servicesMgrForAdmin;
            AuthenticationUri = authenticationUri;
		}

		/// <inheritdoc />
		public IAPM APM { get; }

        /// <inheritdoc />
		public ISyncServiceManager ServicesMgr { get; }
        
        /// <inheritdoc />
		public ISourceServiceFactoryForAdmin ServicesMgrForAdmin { get; }

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