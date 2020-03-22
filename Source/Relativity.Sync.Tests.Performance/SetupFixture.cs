using NUnit.Framework;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.System;
using System.IO;

namespace Relativity.Sync.Tests.Performance
{
	[SetUpFixture]
	public class SetupFixture
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			if (!AppSettings.IsSettingsFileSet)
			{
				throw new FileNotFoundException(
					@"*.runsettings File is not set. Set File in VS -> Test -> Select Settings File. If file doesn't exist generate it by .\Development Scripts\New-TestSettings.ps1.");
			}

			RelativityFacade.Instance.RelyOn<ApiComponent>();

			ARMHelper.CreateInstance();
		}
	}
}
