﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.Conversion;

namespace kCura.IntegrationPoints.Core.Services.EntityManager
{
	public class EntityManagerDataReaderToEnumerableService
	{
		private IObjectBuilder _objectBuilder;
		private string _oldKeyFieldID;
		private string _newKeyFieldID;
		public EntityManagerDataReaderToEnumerableService(IObjectBuilder objectBuilder, string oldKeyFieldID, string newKeyFieldID)
		{
			_objectBuilder = objectBuilder;
			_oldKeyFieldID = oldKeyFieldID;
			_newKeyFieldID = newKeyFieldID;
			ManagerOldNewKeyMap = new Dictionary<string, string>();
		}

		public IDictionary<string, string> ManagerOldNewKeyMap { get; set; }
		public IEnumerable<T> GetData<T>(IDataReader reader)
		{
			try
			{
				//this was not getting me the correct table columns it was giving me some bs column names that made no sense
				//DataColumnCollection columns = reader.GetSchemaTable().Columns;
				//found http://stackoverflow.com/questions/681653/can-you-get-the-column-names-from-a-sqldatareader
				var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
				while (reader.Read())
				{
					string oldKey = reader[_oldKeyFieldID].ToString();
					string newKey = reader[_newKeyFieldID].ToString();
					if (!ManagerOldNewKeyMap.ContainsKey(oldKey)) ManagerOldNewKeyMap.Add(oldKey, newKey);
					yield return _objectBuilder.BuildObject<T>(reader, columns);
				}
			}
			finally
			{
				reader.Dispose();
			}
		}
	}
}