using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class ProviderTypeValidator : IValidator
	{
		private const string _ERROR_PROVIDER_TYPE_INVALID = "Invalid type for given source provider.";
		private const string _RELATIVITY_PROVIDER_GUID = "423b4d43-eae9-4e14-b767-17d629de4bb2";
		private static readonly Guid ExportGuid = new Guid("DBB2860A-5691-449B-BC4A-E18D8519EB3A");
		private static readonly Guid ImportGuid = new Guid("700D94A7-014C-4C7C-B1A2-B53229E3A1C4");

		public Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = new ValidationResult();

			if (string.Equals(configuration.SourceProviderIdentifier, _RELATIVITY_PROVIDER_GUID, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!string.Equals(configuration.TypeIdentifier, ExportGuid.ToString(), StringComparison.InvariantCultureIgnoreCase))
				{
					result.Add(_ERROR_PROVIDER_TYPE_INVALID);
				}
			}
			else
			{
				if (!string.Equals(configuration.TypeIdentifier, ImportGuid.ToString(), StringComparison.InvariantCultureIgnoreCase))
				{
					result.Add(_ERROR_PROVIDER_TYPE_INVALID);
				}
			}

			return Task.FromResult(result);
		}
	}
}