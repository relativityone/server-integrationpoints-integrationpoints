using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Exports.LineFactory
{
	public abstract class LineFactoryBase
	{
		public abstract void WriteLine(System.IO.StreamWriter stream);

		protected LineFactoryBase()
		{
			//Satifies Rule: Abstract types should not have constructors
		}
	}
}
