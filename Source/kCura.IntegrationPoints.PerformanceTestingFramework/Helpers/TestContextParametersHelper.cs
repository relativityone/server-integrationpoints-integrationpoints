using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using File = System.IO.File;

namespace kCura.IntegrationPoints.PerformanceTestingFramework.Helpers
{
	public class TestContextParametersHelper
	{
		private static Dictionary<string, string> _parameters;

		private static void EnsureParametersAreLoaded()
		{
			if (_parameters == null)
			{
				LoadParameters();
			}
		}

		private static void LoadParameters()
		{
			string parametersJsonPath = SharedVariables.AppSettingString("GrazynaRunSettingsJsonPath");
			string json = File.ReadAllText(parametersJsonPath);
			_parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
		}

		public static string GetParameterFromTestContextOrAuxilaryFile(string parameterName)
		{
			string value = TestContext.Parameters[parameterName];
			if (value == null)
			{
				EnsureParametersAreLoaded();
				value = _parameters[parameterName];
				if (value == null)
				{
					throw new TestContextParametersHelperException("Value of the parameter could not be obtained!");
				}
			}

			return value;
		}
	}
}