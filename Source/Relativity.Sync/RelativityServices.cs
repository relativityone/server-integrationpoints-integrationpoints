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
		public RelativityServices(IAPM apm, ISourceServiceFactoryForAdmin servicesMgr, Uri authenticationUri, IHelper helper)
		{
			Helper = helper;
			APM = apm;
            ServicesMgr = servicesMgr;
			AuthenticationUri = authenticationUri;
		}

		/// <inheritdoc />
		public IAPM APM { get; }

		/// <inheritdoc />
        public ISourceServiceFactoryForAdmin ServicesMgr { get; }
		
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