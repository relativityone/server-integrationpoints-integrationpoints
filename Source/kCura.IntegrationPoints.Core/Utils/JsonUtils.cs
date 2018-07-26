using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoints.Core.Utils
{
	public static class JsonUtils
	{
		public static string ReplacePropertyNameIfPresent(string json, string oldPropertyName, string newPropertyName)
		{
			JObject sourceObject = JObject.Parse(json);
			
			JProperty oldProperty = sourceObject.Property(oldPropertyName);
			if (oldProperty == null)
			{
				return json;
			}
			
			oldProperty.Replace(new JProperty(newPropertyName, oldProperty.Value));

			string newJson = JsonConvert.SerializeObject(sourceObject, Formatting.None);
			return newJson;
		}
	}
}
