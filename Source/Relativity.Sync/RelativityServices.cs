using System;
using kCura.WinEDDS.Service.Export;
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
		public RelativityServices(IAPM apm, IServicesMgr servicesMgr, Func<ISearchManager> searchManagerFactory, Uri authenticationUri)
		{
			SearchManagerFactory = searchManagerFactory;
			APM = apm;
			ServicesMgr = servicesMgr;
			AuthenticationUri = authenticationUri;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Func<ISearchManager> SearchManagerFactory { get; }

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