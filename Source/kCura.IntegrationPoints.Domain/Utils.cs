using System;
using System.Text;

namespace kCura.IntegrationPoints.Domain
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
			return new Exception(GetPrintableException(ex));
		}

		public static string GetPrintableException(Exception ex)
		{
			var strBuilder = new StringBuilder();

			if (!String.IsNullOrWhiteSpace(ex.Message))
			{
				strBuilder.AppendLine(ex.Message);
			}

			if (!String.IsNullOrEmpty(ex.StackTrace))
			{
				strBuilder.AppendLine(ex.StackTrace);
			}

			if (ex.InnerException != null)
			{
				strBuilder.Append(GetPrintableException(ex.InnerException));
			}
			return strBuilder.ToString();
		}

		public static string GetFormatForWorkspaceOrJobDisplay(string name, int? id)
		{
			if (id.HasValue)
			{
				return $"{name} - {id}";
			}
			return name;
		}

		public static string GetFormatForWorkspaceOrJobDisplay(string prefix, string name, int id)
		{
			return $"{prefix} - {GetFormatForWorkspaceOrJobDisplay(name, id)}";
		}
	}
}
