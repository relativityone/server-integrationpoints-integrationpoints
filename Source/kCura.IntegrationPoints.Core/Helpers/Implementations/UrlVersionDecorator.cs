using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public static class UrlVersionDecorator
	{
		private static readonly string Version;

		static UrlVersionDecorator()
		{
			Version = Assembly.GetAssembly(typeof(UrlVersionDecorator)).GetName().Version.ToString(4);
		}

		public static string AppendVersion(string url)
		{
			bool alreadyContainsParameters = url.Contains("?") && url.Contains("=");

			string parameterCharacter = alreadyContainsParameters ? "&" : "?";

			return $"{url}{parameterCharacter}v={version}";
		}
	}
}
