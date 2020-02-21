using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;

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

        public FieldObject(RelativityObject serializedObject)
        {
            ArtifactID = serializedObject.ArtifactID;
            Name = Fields.GetFieldValueStringByFieldName(serializedObject, "Name");
            Type = Fields.GetFieldValueStringByFieldName(serializedObject, "Field Type");
            Length = Fields.GetFieldObjectLength(serializedObject);
            Keywords = Fields.GetFieldValueStringByFieldName(serializedObject, "Keywords");
            IsIdentifier = Fields.GetFieldValueBoolByFieldName(serializedObject, "Is Identifier");
            OpenToAssociations = Fields.GetFieldValueBoolByFieldName(serializedObject, "Open To Associations");
        }
    }
    
}

