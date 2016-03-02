using System;
using System.Collections.Generic;
using System.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.DocumentTransferProvider.DataReaders
{
	/// <summary>
	/// Temp class, will be removed as soon as kelper servide of file is done.
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
				String query = String.Format(@"SELECT [DocumentArtifactId], [Location] FROM [EDDSDBO].[FILE] WHERE [DocumentArtifactId] IN ({0})", param);
				using (IDataReader reader = _context.ExecuteSQLStatementAsReader(query))
				{
					while (reader.Read())
					{
						int artifactId = reader.GetInt32(0);
						string location = reader.GetString(1);

						if (String.IsNullOrEmpty(location) == false)
						{
							result[artifactId] = location;
						}
					}
				}
			}
			return result;
		}
	}
}