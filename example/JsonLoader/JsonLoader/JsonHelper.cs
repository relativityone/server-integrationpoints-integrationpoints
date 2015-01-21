﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			return new JsonSettings()
			{
				FieldLocation = @"C:\SourceCode\LDAPSync\example\JsonLoader\JsonLoader\bin\fields.json",
				DataLocation = @"C:\SourceCode\LDAPSync\example\JsonLoader\JsonLoader\bin\data.json"
			};
		}
	}
}
