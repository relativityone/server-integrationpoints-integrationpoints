using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.Web.Models
{
    [DataContract]
    internal class JobActionResult
    {
        public JobActionResult()
        {
        }

        public JobActionResult(IEnumerable<string> errors)
        {
            if (errors != null)
            {
                Errors.AddRange(errors);
            }
        }

        public JobActionResult(string error)
        {
            Errors.Add(error);
        }

        [DataMember]
        public List<string> Errors { get; } = new List<string>();

        [DataMember]
        public bool IsValid => !Errors.Any();
    }
}
