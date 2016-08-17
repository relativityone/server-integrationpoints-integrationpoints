using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources
{
	public static class Data
	{
		public static Stack<IEnumerable<string>> BatchTable { get; set; }
		public static Stack<string> SqlTable { get; set; }

		static Data()
		{
			BatchTable = new Stack<IEnumerable<string>>();
		}

	}
}
