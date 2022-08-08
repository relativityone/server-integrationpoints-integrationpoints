using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.UtilityDTO
{
    public class ResultSet<T>
    {
        public int ResultCount { get; set; }
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
    }
}
