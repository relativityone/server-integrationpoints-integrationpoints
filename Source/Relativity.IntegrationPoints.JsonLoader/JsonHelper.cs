using System.IO;
using Relativity.IntegrationPoints.JsonLoader.Models;

namespace Relativity.IntegrationPoints.JsonLoader
{
	public class JsonHelper
	{
		public virtual string ReadFields(string options)
		{
			JsonSettings settings = GetSettings(options);
			return File.ReadAllText(settings.FieldLocation);
		}

		public virtual string ReadData(string options)
		{
			JsonSettings settings = GetSettings(options);
			return File.ReadAllText(settings.DataLocation);
		}

		public virtual JsonSettings GetSettings(string options)
		{
			JsonSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonSettings>(options);
			return settings;
		}
	}
}
