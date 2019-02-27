﻿using System;

namespace Relativity.Sync.Tests.Unit
{
	internal class FakeAppDomain : IAppDomain
	{
		public event UnhandledExceptionEventHandler UnhandledException;

		public void FireUnhandledException()
		{
			UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(new Exception(), true));
		}
	}
}