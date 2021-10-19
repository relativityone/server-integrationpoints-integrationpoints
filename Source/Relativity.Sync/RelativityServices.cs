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
		public RelativityServices(IAPM apm, ISyncServiceManager servicesMgr, Uri authenticationUri, IHelper helper)
		{
			Helper = helper;
			APM = apm;
			ServicesMgr = servicesMgr;
			AuthenticationUri = authenticationUri;
		}

		/// <inheritdoc />
		public IAPM APM { get; }

		/// <inheritdoc />
		public ISyncServiceManager ServicesMgr { get; }

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