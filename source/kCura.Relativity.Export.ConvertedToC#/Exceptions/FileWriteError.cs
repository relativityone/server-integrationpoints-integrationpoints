using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Exceptions
{
	public class FileWriteException : ExportBaseException
	{
		public enum DestinationFile
		{
			Errors,
			Load,
			Image,
			Generic
		}
		public FileWriteException(DestinationFile destination, System.Exception writeError) : base("Error writing to " + destination.ToString() + " output file", writeError)
		{
		}

	}
}
