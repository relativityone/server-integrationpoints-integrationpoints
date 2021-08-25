using System;
using System.IO;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles
{
	internal static class Natives
	{
		private const string NATIVE_PATH_FORMAT = "Functional\\Helpers\\LoadFiles\\Natives\\{0}";

		public static readonly IDictionary<string, string> NATIVES = new Dictionary<string, string>
		{
			{ "AZIPPER_0007291", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007291.htm")) },
			{ "AZIPPER_0007293", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007293.htm")) },
			{ "AZIPPER_0007299", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007299.htm")) },
			{ "AZIPPER_0007300", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007300.htm")) },
			{ "AZIPPER_0007491", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007491.htm")) },
			{ "AZIPPER_0007494", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007494.htm")) },
			{ "AZIPPER_0007495", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007495.htm")) },
			{ "AZIPPER_0007746", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007746.htm")) },
			{ "AZIPPER_0007747", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007747.dat")) },
			{ "AZIPPER_0007748", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format(NATIVE_PATH_FORMAT, "AZIPPER_0007748.dat")) }
		};

		public static IDictionary<string, string> GenerateNativesForLoadFileImport()
        {
			IDictionary<string, string> natives = new Dictionary<string, string>();
			for(int i=1; i<10; i++)
            {
				string line = String.Format("^10_DOCS_WITH_NATIVES_000000000{0}^|^.\\Natives\\AZIPPER_0007291.htm^|^206.00^|^AZIPPER_0007291.htm^|^FOLDER_{1}^", i, i);
				natives.Add($"1M_DOCS_WITH_NATIVES_000000000{i}", line);
			}

			return natives;
        }
	}
}
