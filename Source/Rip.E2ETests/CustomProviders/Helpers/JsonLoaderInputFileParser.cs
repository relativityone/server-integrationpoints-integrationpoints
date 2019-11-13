using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.Data;
using Moq;
using Relativity.API;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class JsonLoaderInputFileParser
	{
		public static Dictionary<string, string> GetNameToSampleTextForInputFilesMapping(string inputFilePath)
		{
            Mock<IAPILog> logger = new Mock<IAPILog>();
            IntegrationPointSerializer serializer =new IntegrationPointSerializer(logger.Object);
            var jsonData = serializer.Deserialize<JsonLoaderData[]>(File.ReadAllText(inputFilePath));
            var nameToTextDictionary = new Dictionary<string, string>();
            foreach (var jsonLoaderObject in jsonData)
            {
                nameToTextDictionary[jsonLoaderObject.ID0] = jsonLoaderObject.ID1;
            }
            return nameToTextDictionary;
        }
	}
}
