using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export
{
	public class Settings
	{
		/// -----------------------------------------------------------------------------
		/// <summary>
		///		Default timeout wait time for Web Service in Milliseconds.
		///		Set to 1 minute (2005-08-31).
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[nkapuza]	8/31/2005	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public static Int32 DefaultTimeOut = Config.WebAPIOperationTimeout;
		public static string AuthenticationToken = string.Empty;
		public const Int32 MAX_STRING_FIELD_LENGTH = 1048576;
	    public const int EMPTY_ARRAY = 0;
		public static bool SendEmailOnLoadCompletion = false;
	}
}
