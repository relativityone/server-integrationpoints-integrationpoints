using Relativity.Telemetry.APM;
using System;
using Relativity.API;
using Relativity.Sync.KeplerFactory;

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
		///     Used to create handles to Relativity Services
		/// </summary>
		Uri AuthenticationUri { get; }


		/// <summary>
		///     Relativity authentication endpoint address
		/// </summary>
        ISyncServiceManager ServicesMgr { get; }

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