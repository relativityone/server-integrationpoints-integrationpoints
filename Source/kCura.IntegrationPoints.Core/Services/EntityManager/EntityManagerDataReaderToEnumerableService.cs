using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.Conversion;

namespace kCura.IntegrationPoints.Core.Services.EntityManager
{
    public class EntityManagerDataReaderToEnumerableService
    {
        private readonly IObjectBuilder _objectBuilder;
        private readonly string _oldKeyFieldID;
        private readonly string _newKeyFieldID;
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
            while (reader.Read())
            {
                string oldKey = reader[_oldKeyFieldID].ToString();
                string newKey = reader[_newKeyFieldID].ToString();
                if (!ManagerOldNewKeyMap.ContainsKey(oldKey))
                {
                    ManagerOldNewKeyMap.Add(oldKey, newKey);
                }

                yield return _objectBuilder.BuildObject<T>(reader);
            }
        }
    }
}
