using System;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class ResultSetExtensions
	{
		/// <summary>
		/// Verifies ResultSet
		/// </summary>
		/// <typeparam name="T">Type of ResultSet object</typeparam>
		/// <param name="resultSet">ResultSet object</param>
		public static void VerifyResultSet<T>(this ResultSet<T> resultSet) where T : Artifact
		{
			if (!resultSet.Success)
			{
				throw new Exception($"Unable to read {typeof(T)}: {resultSet.Message}");
			}
		}
	}
}
