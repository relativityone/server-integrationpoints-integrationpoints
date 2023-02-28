using Relativity.IntegrationPoints.FieldsMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace kCura.IntegrationPoints.Web.Models
{
    public class ClassifiedFieldDTO
    {
        public ClassificationLevel ClassificationLevel { get; set; }

        public string ClassificationReason { get; set; }

        public string FieldIdentifier { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public int Length { get; set; }

        public bool IsIdentifier { get; set; }

        public bool IsRequired { get; set; }

        // to be compatible with old JS code
        public string DisplayName => Name;

        // to be compatible with old JS code
        public string ActualName => Name + (string.IsNullOrEmpty(Type) ? "" : $" [{Type}]");

        public ClassifiedFieldDTO()
        {
        }

        public ClassifiedFieldDTO(FieldClassificationResult result)
        {
            ClassificationLevel = result.ClassificationLevel;
            ClassificationReason = result.ClassificationReason;
            FieldIdentifier = result.FieldInfo.FieldIdentifier;
            Name = result.FieldInfo.Name;
            Type = result.FieldInfo.DisplayType;
            Length = result.FieldInfo.Length;
            IsIdentifier = result.FieldInfo.IsIdentifier;
            IsRequired = result.FieldInfo.IsRequired;
        }
    }
}
