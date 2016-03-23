using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Exceptions
{
	public abstract class ExportBaseException : System.Exception
	{
		protected ExportBaseException(string message, System.Exception innerException) : base(message, innerException)
		{
		}
	}
}
