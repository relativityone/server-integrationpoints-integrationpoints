namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class DocumentFieldInfo
	{
		public string Name { get; }
		public string FieldIdentifier { get; }
		public string Type { get; }
		public int Length { get; }

		public bool IsIdentifier { get; set; }
		public bool IsRequired { get; set; }
		public bool? OpenToAssociations { get; set; }
		public string AssociativeObjectType { get; set; }

		public string DisplayType => Type.Equals(FieldTypeName.FIXED_LENGTH_TEXT) && Length != 0 ? $"{Type}({Length})" : Type;

		public DocumentFieldInfo(string fieldIdentifier, string name, string type, int length = 0)
		{
			FieldIdentifier = fieldIdentifier;
			Name = name;
			Type = type ?? string.Empty;

			Length = TryGetLengthIfTypeExtendend(out int extendedLength) ? extendedLength : length;
		}

		public bool IsTypeCompatible(DocumentFieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				return false;
			}

			// this is here to support CustomProviders that do not feed field types
			if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(fieldInfo.Type))
			{
				return true;
			}

			if (Type.StartsWith(FieldTypeName.FIXED_LENGTH_TEXT))
			{
				if (fieldInfo.Type == FieldTypeName.LONG_TEXT ||
				    (fieldInfo.Type.StartsWith(FieldTypeName.FIXED_LENGTH_TEXT) &&
				     (Length <= fieldInfo.Length || fieldInfo.Length == 0))
				)
				{
					return true;
				}

				return false;
			}

			if (Type == fieldInfo.Type)
			{
				return true;
			}

			return false;
		}

		public bool IsObjectType() => Type == FieldTypeName.SINGLE_OBJECT || Type == FieldTypeName.MULTIPLE_OBJECT;

		private bool TryGetLengthIfTypeExtendend(out int length)
		{
			length = 0;
			if (Type != null && Type.Contains("(") && Type.Contains(")")
				&& int.TryParse(Type.Substring(Type.IndexOf('(') + 1, Type.IndexOf(')') - Type.IndexOf('(') - 1), out length))
			{
				return true;
			}

			return false;
		}
	}
}