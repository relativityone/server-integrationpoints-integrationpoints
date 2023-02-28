using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain
{
    public class ApplicationBinary : IComparer<ApplicationBinary>
    {
        public int ArtifactID { get; set; }

        public string Name { get; set; }

        public byte[] FileData { get; set; }

        public int Compare(ApplicationBinary x, ApplicationBinary y)
        {
            if (x.ArtifactID.CompareTo(y.ArtifactID) != 0)
            {
                return x.ArtifactID.CompareTo(y.ArtifactID);
            }
            else if (String.Compare(x.Name, y.Name, StringComparison.Ordinal) != 0)
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            }
            else
            {
                return 0;
            }
        }
    }
}
