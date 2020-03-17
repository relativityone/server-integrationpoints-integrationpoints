using System.Linq;
using System.Threading.Tasks;
using Relativity;
using Relativity.API;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal sealed class UserFieldSanitizer : IExportFieldSanitizer
	{
		private readonly IHelper _helper;
		private readonly ISanitizationDeserializer _sanitizationDeserializer;

		public FieldTypeHelper.FieldType SupportedType { get; } = FieldTypeHelper.FieldType.User;

		public UserFieldSanitizer(IHelper helper, ISanitizationDeserializer sanitizationDeserializer)
		{
			_helper = helper;
			_sanitizationDeserializer = sanitizationDeserializer;
		}

		public async Task<object> SanitizeAsync(int workspaceArtifactID, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			if (initialValue is null)
			{
				return initialValue;
			}

			int userArtifactId = GetUserArtifactId(itemIdentifier, sanitizingSourceFieldName, initialValue);

			using (IUserInfoManager userInfoManager = _helper.GetServicesManager().CreateProxy<IUserInfoManager>(ExecutionIdentity.System))
			{
				QueryRequest userQuery = new QueryRequest
				{
					Condition = $@"('ArtifactID' == {userArtifactId})"
				};

				UserInfoQueryResultSet users = await userInfoManager.RetrieveUsersBy(-1, userQuery, 0, 1).ConfigureAwait(false);
				if (users?.ResultCount == 1)
				{
					return users.DataResults.Single().Email;
				}
			}

			throw new InvalidExportFieldValueException(itemIdentifier, sanitizingSourceFieldName, $"Could not retrieve info for user with ArtifactID {userArtifactId}.");
		}

		private int GetUserArtifactId(string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			UserInfo userFieldValue = _sanitizationDeserializer.DeserializeAndValidateExportFieldValue<UserInfo>(
				itemIdentifier,
				sanitizingSourceFieldName,
				initialValue);

			return userFieldValue.ArtifactID;
		}
	}
}
