using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Utils;
using Relativity.Sync.KeplerFactory;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Newtonsoft.Json;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Returns an user's email given its exported representation.
	/// Import API expects the email address instead of the ArtifactID.
	/// </summary>
	internal sealed class UserFieldSanitizer : IExportFieldSanitizer
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly JSONSerializer _serializer = new JSONSerializer();

		public RelativityDataType SupportedType { get; } = RelativityDataType.User;

		public UserFieldSanitizer(ISourceServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<object> SanitizeAsync(int workspaceArtifactId, string itemIdentifierSourceFieldName, string itemIdentifier, string sanitizingSourceFieldName, object initialValue)
		{
			if (initialValue is null)
			{
				return initialValue;
			}

			int userArtifactId = GetUserArtifactId(initialValue);
			
			using (IUserInfoManager userInfoManager = await _serviceFactory.CreateProxyAsync<IUserInfoManager>().ConfigureAwait(false))
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

			throw new InvalidExportFieldValueException($"Could not retrieve info for user with ArtifactID {userArtifactId}. If this workspace was restored using ARM, verify if " +
				$"user has been properly mapped during workspace restore.");
		}

		private int GetUserArtifactId(object initialValue)
		{
			UserInfo userFieldValue;
			try
			{
				userFieldValue = _serializer.Deserialize<UserInfo>(initialValue.ToString());
			}
			catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
			{
				throw new InvalidExportFieldValueException($"Expected value to be deserializable to {typeof(UserInfo)}, but instead type was {initialValue.GetType()}.", ex);
			}

			return userFieldValue.ArtifactID;
		}
	}
}
