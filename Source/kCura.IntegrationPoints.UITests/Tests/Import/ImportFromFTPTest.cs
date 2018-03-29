using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.Import
{
	public class ImportFromFTPTest : UiTest
	{
		protected override bool InstallLegalHoldApp => true;

		[Test, Order(1)]
		public void Test()
		{
		}
	}
}
