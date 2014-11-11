using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace kCura.LDAPSync.prototype.datasources.Implementations
{
	public class JsonFieldReader : IFieldQuery
	{
		private readonly string _file;
		public JsonFieldReader(string file)
		{
			_file = file;
		}

		private class Structure
		{
			public List<string> Fields { get; set; } 
		}

		public IEnumerable<FieldEntry> GetFields()
		{
			using (StreamReader r = new StreamReader(_file))
			{
				string json = r.ReadToEnd();
				var items = JsonConvert.DeserializeObject<Structure>(json);
				return items.Fields.Select(x=>new FieldEntry
				{
					DisplayName = x,
					FieldIdentifier = x,
					FieldType = FieldType.String
				});
			}
			
		}
	}
}
