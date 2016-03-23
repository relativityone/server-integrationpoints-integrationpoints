using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Exceptions
{
	public class WebDownloadCorruptException : System.Exception
	{
		public WebDownloadCorruptException(string message) : base(message)
		{
		}
	}
}
