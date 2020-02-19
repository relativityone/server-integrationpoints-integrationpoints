using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	public class FieldObject
	{
		public int ArtifactID { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
        public string Keywords { get; set; }
		public bool IsIdentifier { get; set; }
        public bool OpenToAssociations { get; set; }
        public int Length { get; set; }

        public string DisplayType
        {
            get
            {
                string fixedLengthText = "Fixed-Length Text";
                if (Type.Equals(fixedLengthText))
                {
                    return $"{Type}({Length})";
                }

                return Type;
            }
        }

	}
}

