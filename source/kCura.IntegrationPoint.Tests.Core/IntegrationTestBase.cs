using System;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class IntegrationTestBase
	{
		protected IntegrationTestBase()
		{
			GerronHelper = new Helper();
			_help = new Lazy<IHelper>(() => new TestHelper(GerronHelper));
		}

		public IHelper Helper => _help.Value;
		private readonly Lazy<IHelper> _help;

		public Helper GerronHelper { get; }
	}
}