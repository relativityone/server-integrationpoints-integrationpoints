using System.IO;
using JsonLoader.Models;

namespace JsonLoader
{
	public class JsonHelper
	{
		public virtual string ReadFields(string options)
		{
			var settings = GetSettings(options);
			return File.ReadAllText(settings.FieldLocation);
		}

		public virtual string ReadData(string options)
		{
			var settings = GetSettings(options);
			return File.ReadAllText(settings.DataLocation);
		}

		public virtual JsonSettings GetSettings(string options)
		{
			var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonSettings>(options);
			return settings;
		}
	}
}
