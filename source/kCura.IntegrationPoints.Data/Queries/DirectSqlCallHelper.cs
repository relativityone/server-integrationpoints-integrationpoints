using System;
using System.Collections.Generic;
using System.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Queries
{
	/// <summary>
	/// IMPORTANT : 
	/// This is a temporary class that is used to query data of which no relativity api is supported.
	/// To fix this, we will need to introduce/modify kelper service(s) functionalities to aquire this data.
	/// </summary>
	public class DirectSqlCallHelper
	{
		private readonly IDBContext _context;

		public DirectSqlCallHelper(IDBContext context)
		{
			_context = context;
		}

		public Dictionary<int, string> GetFileLocation(int[] docArtifactIds)
		{
			Dictionary<int, string> result = new Dictionary<int, string>();
			if (docArtifactIds.Length > 0)
			{
				string param = String.Join(",", docArtifactIds);
				String query = String.Format(@"SELECT [DocumentArtifactId], [Location] FROM [EDDSDBO].[FILE] WHERE [Type] = 0 AND [DocumentArtifactId] IN ({0})", param);
				using (IDataReader reader = _context.ExecuteSQLStatementAsReader(query))
				{
					while (reader.Read())
					{
						int artifactId = reader.GetInt32(0);
						string location = reader.GetString(1);
						result[artifactId] = location;
					}
				}
			}
			return result;
		}
	}
}