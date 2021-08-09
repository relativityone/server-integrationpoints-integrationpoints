using System;
using Relativity.Telemetry.APM;

namespace Relativity.Sync
{
	/// <inheritdoc />
	public sealed class RelativityServices : IRelativityServices
	{
		/// <summary>
		///     Constructor
		/// </summary>
		public RelativityServices(IAPM apm, ISyncServiceManager servicesMgr, Uri authenticationUri)
		{
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
	}
}