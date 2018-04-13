using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.ImportFromLoadFile
{
	public class ImportFromLoadFileTest : UiTest
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			CopyFilesToFileshare();
		}

		private void CopyFilesToFileshare()
		{
			// TODO
		}

		[Test, Order(1)]
		public void DocumentImportFromLoadFile_TC_ILF_DOC_1()
		{
			// TODO
		}
	}
}
