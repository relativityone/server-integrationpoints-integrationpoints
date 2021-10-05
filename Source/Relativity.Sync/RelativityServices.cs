using System;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace Relativity.Sync
{
	/// <inheritdoc />
	public sealed class RelativityServices : IRelativityServices
	{
		private readonly IHelper _helper;

		/// <summary>
		///     Constructor
		/// </summary>
		public RelativityServices(IAPM apm, ISyncServiceManager servicesMgr, Uri authenticationUri, IHelper helper)
		{
			_helper = helper;
			APM = apm;
			ServicesMgr = servicesMgr;
			AuthenticationUri = authenticationUri;
		}

		/// <inheritdoc />
		public IAPM APM { get; }

		/// <inheritdoc />
		public ISyncServiceManager ServicesMgr { get; }

		/// <inheritdoc />
		public Uri AuthenticationUri { get; }

		/// <inheritdoc />
		public IDBContext GetEddsDbContext()
		{
			return _helper.GetDBContext(-1);
		}

	}
}