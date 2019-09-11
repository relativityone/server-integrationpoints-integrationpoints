using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryTemplate
{
	public class Class2
	{
		public Class1 Namer { get; set; }

		public Class2()
		{
		}

		public Class2(Class1 namer)
		{
			this.Namer = namer;
		}

		public string GetName()
		{
			return Namer != null ? Namer.Name : "I have no name.";
		}
	}
}
