using System;
using Relativity.API;

namespace Relativity.Sync.Tests.System.Stubs
{
	public class ProvideServiceUrisStub : IProvideServiceUris
	{
		public Uri RestUri()
		{
			return AppSettings.RelativityRestUrl;
		}

		public Uri RSAPIUri()
		{
			return AppSettings.RelativityServicesUrl;
		}

		public Uri AuthenticationUri()
		{
			return AppSettings.RelativityUrl;
		}
	}
}