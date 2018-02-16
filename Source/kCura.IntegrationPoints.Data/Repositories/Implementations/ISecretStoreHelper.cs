using System.Collections.Generic;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public interface ISecretStoreHelper
	{
		void SetEncryptedSecuredConfigurationForNewRdo(IEnumerable<FieldRefValuePair> fieldValues);
		void SetEncryptedSecuredConfigurationForExistingRdo(IntegrationPoint existingRdo, IEnumerable<FieldRefValuePair> fieldValues, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
		string DecryptSecuredConfiguration(string secretId);
	}
}