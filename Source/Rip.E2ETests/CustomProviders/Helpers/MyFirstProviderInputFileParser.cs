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
			var testRecords = myFirstProviderInputData
				.Descendants("document")
				.Select(document => new
				{
					Name = document.Element("Name")?.Value,
					Text = document.Element("Text")?.Value
				});

			var nameToTextDictionary = new Dictionary<string, string>();
			foreach (var testRecord in testRecords)
			{
				AddTestRecordsToDictionarySkippingDuplicates(
					nameToTextDictionary,
					testRecord.Name,
					testRecord.Text);
			}
			return nameToTextDictionary;
		}

		private static void AddTestRecordsToDictionarySkippingDuplicates(
			IDictionary<string, string> nameToTextDictionary,
			string name,
			string text)
		{
			if (!nameToTextDictionary.ContainsKey(name))
			{
				nameToTextDictionary[name] = text;
			}
		}
	}
}
