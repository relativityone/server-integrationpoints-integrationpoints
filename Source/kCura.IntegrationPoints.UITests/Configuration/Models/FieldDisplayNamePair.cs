using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.UITests.Configuration.Models
{
    public class FieldDisplayNamePair
    {
        public string SourceDisplayName { get; set; }
        public string DestinationDisplayName { get; set; }

        public FieldDisplayNamePair(string _sourceDisplayName, string _destinationDisplayName)
        {
            SourceDisplayName = _sourceDisplayName;
            DestinationDisplayName = _destinationDisplayName;
        }

        public FieldDisplayNamePair(FieldMapModel fieldPair)
        {
            SourceDisplayName = fieldPair.SourceFieldObject.DisplayName;
            DestinationDisplayName = fieldPair.DestinationFieldObject.DisplayName;
        }
    }
}
