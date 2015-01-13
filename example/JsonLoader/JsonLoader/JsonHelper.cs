using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonLoader
{
	public class JsonHelper
	{
		public virtual string ReadFile(string options)
		{
			var settings = GetSettings(options);
			return File.ReadAllText(settings.FileName);
		}

		public virtual JsonSettings GetSettings(string options)
		{
			var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonSettings>(options);
			return settings;
		}
	}
}
