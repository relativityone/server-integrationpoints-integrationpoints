using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromeOptionsExtensions
	{
		public static ChromeOptions AddAdditionalCapabilities(this ChromeOptions value)
		{
			if (SharedVariables.UiOptionsAdditionalCapabilitiesAcceptSslCertificates)
			{
				value.AddAdditionalCapability(CapabilityType.AcceptSslCertificates, true, true);
			}
			if (SharedVariables.UiOptionsAdditionalCapabilitiesAcceptInsecureCertificates)
			{
				value.AddAdditionalCapability(CapabilityType.AcceptInsecureCertificates, true, true);
			}
			return value;
		}

		public static void AddBrowserOptions(this ChromeOptions value)
		{
			var optionsFromAppConfig = new Dictionary<string, bool>
			{
				// Disables "Chrome is being controlled by automated test software." bar
				["disable-infobars"] = SharedVariables.UiOptionsArgumentsDisableInfobars,
				["headless"] = SharedVariables.UiOptionsArgumentsHeadless,
				["ignore-certificate-errors"] = SharedVariables.UiOptionsArgumentsIgnoreCertificateErrors,
				["no-sandbox"] = SharedVariables.UiOptionsArgumentsNoSandbox
			};

			foreach (KeyValuePair<string, bool> option in optionsFromAppConfig.Where(x => x.Value))
			{
				value.AddArgument(option.Key);
			}
		}
	}
}