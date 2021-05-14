using System;
using Relativity.Telemetry.APM;

namespace Relativity.Sync
{
	/// <summary>
	///     Provides access to Relativity Services
	/// </summary>
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

		/// <summary>
		///     Provides access to Relativity Telemetry
		/// </summary>
		public IAPM APM { get; }

		/// <summary>
		///     Used to create handles to Relativity Services
		/// </summary>
		public ISyncServiceManager ServicesMgr { get; }

		/// <summary>
		///     Relativity authentication endpoint address
		/// </summary>
		public Uri AuthenticationUri { get; }
	}
}