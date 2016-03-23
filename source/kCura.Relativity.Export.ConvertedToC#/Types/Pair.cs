using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Types
{
	[Serializable()]
	public class Pair
	{
		public string Value;
		public string Display;
		public override string ToString()
		{
			return this.Display;
		}
		public Pair(string v, string d)
		{
			this.Value = v;
			this.Display = d;
		}
		public new bool @equals(Pair other)
		{
			return (this.Display == other.Display & this.Value == other.Value);
		}
	}
}
