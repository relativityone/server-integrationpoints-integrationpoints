using Relativity.Telemetry.APM;
using System;

namespace Relativity.Sync
{
	public interface IRelativityServices
	{
		IAPM APM { get; }
		Uri AuthenticationUri { get; }
		ISyncServiceManager ServicesMgr { get; }
	}
}