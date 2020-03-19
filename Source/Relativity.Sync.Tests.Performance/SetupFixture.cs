﻿using NUnit.Framework;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Sync.Tests.Performance.ARM;

namespace Relativity.Sync.Tests.Performance
{
	[SetUpFixture]
	public class SetupFixture
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			if (!TestSettingsConfig.IsSettingsFileSet)
			{
				throw new System.IO.FileNotFoundException(
					@"*.runsettings File is not set. Set File in VS -> Test -> Select Settings File. If file doesn't exist generate it by .\Development Scripts\New-TestSettings.ps1.");
			}

			RelativityFacade.Instance.RelyOn<ApiComponent>();

			ARMHelper.CreateInstance();
		}
	}
}
