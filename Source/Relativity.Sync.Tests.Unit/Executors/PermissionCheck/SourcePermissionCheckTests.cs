using NUnit.Framework;
using NUnit.Framework.Internal;
using Relativity.Sync.Executors.PermissionCheck;

namespace Relativity.Sync.Tests.Unit.Executors.PermissionCheck
{
	[TestFixture]
	public class SourcePermissionCheckTests
	{
		private SourcePermissionCheck _instance;

		[SetUp]
		public void SetUp()
		{
			

			_instance = new SourcePermissionCheck();
		}
	}
}