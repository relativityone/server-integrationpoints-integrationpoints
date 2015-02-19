using System;
using System.Text;

namespace kCura.IntegrationPoints.Core
{
	/// <summary>
	/// 
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Generates a no custom exception that can be passed to the host proccess to be proccessed
		/// </summary>
		/// <param name="ex">The exception that was raised.</param>
		/// <returns>A generic exception that can be passed to the host process.</returns>
		public static Exception GetNonCustomException(System.Exception ex)
		{
			var strBuilder = new StringBuilder();

			if (!String.IsNullOrWhiteSpace(ex.Message))
			{
				strBuilder.Append(ex.Message);
			}

			if (!String.IsNullOrEmpty(ex.StackTrace))
			{
				strBuilder.AppendLine(ex.StackTrace);
			}

			if (ex.InnerException != null)
			{
				strBuilder.AppendLine(ex.InnerException.ToString());
			}

			return new Exception(strBuilder.ToString());
		}
	}
}
