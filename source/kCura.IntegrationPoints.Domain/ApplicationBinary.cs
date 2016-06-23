using System.Collections.Generic;

namespace kCura.IntegrationPoints.Contracts.Domain
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
            else if (x.Name.CompareTo(y.Name) != 0)
            {
                return x.Name.CompareTo(y.Name);
            }
            else
            {
                return 0;
            }
        }
    }
}
