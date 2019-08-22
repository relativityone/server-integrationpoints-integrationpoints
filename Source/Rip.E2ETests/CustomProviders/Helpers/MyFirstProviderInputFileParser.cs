using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class MyFirstProviderInputFileParser
	{
		public static Dictionary<string, string> GetNameToTextForInputFilesMapping(string inputFilePath)
		{
			XElement myFirstProviderInputData = XElement.Load(inputFilePath);
			var zzzz = myFirstProviderInputData
				.Descendants("document")
				.Select(document => new
				{
					Name = document.Element("Name")?.Value,
					Text = document.Element("Text")?.Value
				});

			var nameToTextDictionary = new Dictionary<string, string>();
			foreach (var x in zzzz)
			{
				if (!nameToTextDictionary.ContainsKey(x.Name)) // we want to skip duplicate entries
				{
					nameToTextDictionary[x.Name] = x.Text;
				}
			}

			return nameToTextDictionary;
		}
	}
}
