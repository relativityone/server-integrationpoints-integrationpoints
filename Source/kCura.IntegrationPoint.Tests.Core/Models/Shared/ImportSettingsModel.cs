using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoint.Tests.Core.Models.Shared
{
    public class ImportSettingsModel
    {
        public ImportSettingsModel()
        {
            FieldMapping = new List<Tuple<string, string>>();
        }

        public bool MapFieldsAutomatically { get; set; }

        public List<Tuple<string, string>> FieldMapping { get; set; }

        public OverwriteType Overwrite { get; set; }
    }
}
