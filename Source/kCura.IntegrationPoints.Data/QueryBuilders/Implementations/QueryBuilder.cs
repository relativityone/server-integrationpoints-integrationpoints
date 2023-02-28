using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
    public abstract class QueryBuilder
    {
        protected IList<string> Conditions { get; }

        protected List<FieldRef> Fields { get; set; }

        protected QueryBuilder()
        {
            Conditions = new List<string>();
            Fields = new List<FieldRef>();
        }

        public abstract QueryRequest Build();

        protected string BuildCondition()
        {
            if (Conditions.Count == 0)
            {
                return null;
            }
            return string.Join(" AND ", Conditions);
        }
    }
}
