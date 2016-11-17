using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class FieldMappingsValidator : IValidator
	{
		private readonly ISerializer _serializer;
		public string Key => Constants.IntegrationPoints.Validation.FIELD_MAP;

		public FieldMappingsValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public ValidationResult Validate(object value)
		{
			var fieldMappings = value as string;

			List<string> invalidFieldMappingList = new List<string>();
			var fieldMaps = _serializer.Deserialize<IEnumerable<FieldMap>>(fieldMappings);

			foreach (FieldMap fieldMap in fieldMaps)
			{
				//TODO Check if there is at least one identifier	
			}

			//TODO
			return new ValidationResult { IsValid = true };
		}
	}
}