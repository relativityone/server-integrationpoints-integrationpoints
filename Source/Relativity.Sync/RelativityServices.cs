using System;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace Relativity.Sync
{
	/// <summary>
	///     Provides access to Relativity Services
	/// </summary>
	public sealed class RelativityServices
	{
		/// <summary>
		///     Constructor
		/// </summary>
		public RelativityServices(IAPM apm, IServicesMgr servicesMgr, Uri authenticationUri)
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
		public IServicesMgr ServicesMgr { get; }

		/// <summary>
		///     Relativity authentication endpoint address
		/// </summary>
		public Uri AuthenticationUri { get; }
	}
}