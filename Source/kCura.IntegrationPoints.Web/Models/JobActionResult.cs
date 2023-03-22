using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.Web.Models
{
    [DataContract]
    internal class JobActionResult
    {
        [DataMember]
        public List<string> Errors { get; } = new List<string>();

        [DataMember]
        public bool IsValid => !Errors.Any();
    }
}
