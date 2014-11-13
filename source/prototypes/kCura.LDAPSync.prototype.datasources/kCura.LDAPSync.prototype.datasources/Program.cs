using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using kCura.LDAPSync.prototype.datasources.Extensions;
using kCura.LDAPSync.prototype.datasources.Implementations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.LDAPSync.prototype.datasources
{
	public class Program
	{
		private static IEnumerable<FieldEntry> GetDataSourceFields()
		{
			var ls = new JsonFieldReader("fields.json");
			var fields = ls.GetFields();
			return fields;
		}

		private static IEnumerable<FieldEntry> GetOutputFields()
		{
			var ls = new JsonFieldReader("destinationFields.json");
			var fields = ls.GetFields();
			return fields;
		}

		public static void Main(string[] args)
		{
			var manager = new Manager(new DataSourceFactory());
			manager.Execute();

			var worker = new Worker(new DataConverterFactory(), new DataSourceFactory());
			worker.Execute();
		}
		
	}
}
