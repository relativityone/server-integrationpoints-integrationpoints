using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class SplitJsonObjectService : ISplitJsonObjectService
	{
		public SplittedJsonObject Split(string jsonString, params string[] propertiesToExtract)
		{
			if (string.IsNullOrEmpty(jsonString))
			{
				return null;
			}

			try
			{
				JObject sourceObject = JObject.Parse(jsonString);

				JObject objectWithExtractedProperties = CopyPropertiesToNewObject(sourceObject, propertiesToExtract);

				RemovePropertiesFromObject(sourceObject, propertiesToExtract);

				return SerializeAndReturnResult(sourceObject, objectWithExtractedProperties);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private void RemovePropertiesFromObject(JObject sourceObject, string[] propertiesToExtract)
		{
			foreach (string propertyName in propertiesToExtract)
			{
				sourceObject.Remove(propertyName);
			}
		}

		private JObject CopyPropertiesToNewObject(JObject sourceObject, string[] propertiesToExtract)
		{
			var objectWithExtractedProperties = new JObject();

			foreach (string propertyName in propertiesToExtract)
			{
				JProperty property = sourceObject.Property(propertyName);
				if (property != null)
				{
					objectWithExtractedProperties[property.Name] = property.Value;
				}
			}

			return objectWithExtractedProperties;
		}

		private static SplittedJsonObject SerializeAndReturnResult(JObject sourceObject, JObject objectWithExtractedProperties)
		{
			string jsonWithoutExtractedProperties = JsonConvert.SerializeObject(sourceObject, Formatting.None);
			string jsonWithExtractedProperties = JsonConvert.SerializeObject(objectWithExtractedProperties, Formatting.None);

			return new SplittedJsonObject
			{
				JsonWithoutExtractedProperties = jsonWithoutExtractedProperties,
				JsonWithExtractedProperties = jsonWithExtractedProperties
			};
		}
	}
}