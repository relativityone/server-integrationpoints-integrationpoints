using Relativity.Telemetry.APM;
using System;
using Relativity.API;

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
		///     Relativity EDDS DbContext
		/// </summary>
		IDBContext GetEddsDbContext();
	}
}