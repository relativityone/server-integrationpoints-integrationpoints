using System;

namespace Relativity.Sync
{
	internal interface IAppDomain
	{
		event UnhandledExceptionEventHandler UnhandledException;
	}
}