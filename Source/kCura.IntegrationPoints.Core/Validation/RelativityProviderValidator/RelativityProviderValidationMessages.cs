using System;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public class RelativityProviderValidationMessages
	{
		public static readonly string FIELD_MAP_DESTINATION_FIELD_NOT_MAPPED = "All selected fields must be mapped. Destination field not mapped to Source: ";
		public static readonly string FIELD_MAP_FIELD_IS_IDENTIFIER = "Is Identifier";
		public static readonly string FIELD_MAP_FIELD_MUST_BE_MAPPED = "must be mapped.";
		public static readonly string FIELD_MAP_FIELD_NAME = "Name";
		public static readonly string FIELD_MAP_FIELD_NOT_EXIST_IN_DESTINATION_WORKSPACE = "Field does not exist in destination workspace: ";
		public static readonly string FIELD_MAP_FIELD_NOT_EXIST_IN_SOURCE_WORKSPACE = "Field does not exist in source workspace: ";
		public static readonly string FIELD_MAP_IDENTIFIERS_NOT_MATCHED = "Identifier must be mapped with another identifier.";
		public static readonly string FIELD_MAP_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED = "All selected fields must be mapped. Destination and Source fields not mapped.";
		public static readonly string FIELD_MAP_SOURCE_FIELD_NOT_MAPPED = "All selected fields must be mapped. Source field not mapped to Destination: ";
		public static readonly string FIELD_MAP_UNIQUE_IDENTIFIER_MUST_BE_MAPPED = "The unique identifier must be mapped.";

		public static readonly string SAVED_SEARCH_NOT_EXIST = "Saved Search does not exist.";

		public static readonly string WORKSPACE_INVALID_NAME = "workspace name contains an invalid character.";
		public static readonly string WORKSPACE_NOT_EXIST = "workspace does not exist.";					
	}
}