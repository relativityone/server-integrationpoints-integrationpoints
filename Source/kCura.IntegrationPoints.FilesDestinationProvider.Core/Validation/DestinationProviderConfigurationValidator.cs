using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class DestinationProviderConfigurationValidator : IValidator
	{
		private readonly ISerializer _serializer;

		public string Key => IntegrationPoints.Core.Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID;

		public DestinationProviderConfigurationValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public ValidationResult Validate(object value)
		{
			var settings = _serializer.Deserialize<ExportSettings>(value.ToString());

			// TODO implement validation (doh!)

			return new ValidationResult { IsValid = true };
		}
	}
}