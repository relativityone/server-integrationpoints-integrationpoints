using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Group;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Permission;
using Relativity.Services.RestApi.Client;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using User = Relativity.Services.User.User;
using UserRef = Relativity.Services.User.UserRef;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class NativeCopyLinksValidator : IValidator
	{
		private const string _COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION = 
			"You do not have permission to run this import because it uses referential links to files. " + 
			"You must either log in as a system administrator or change the settings to upload files to run this import.";

		private readonly IInstanceSettings _instanceSettings;
		private readonly IUserContextConfiguration _userContext;
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly ISyncLog _logger;

		public NativeCopyLinksValidator(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactory,  ISyncLog logger)
		{
			_instanceSettings = instanceSettings;
			_userContext = userContext;
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating Native File links copy Restriction");

			var validationResult = new ValidationResult();

			try
			{
				if (configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.SetFileLinks)
				{
					return validationResult;
				}

				bool isRestrictReferentialFileLinksOnImport = await _instanceSettings.GetRestrictReferentialFileLinksOnImportAsync().ConfigureAwait(false);
				bool executingUserIsAdmin = await ExecutingUserIsAdminAsync(configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);
				if (isRestrictReferentialFileLinksOnImport && !executingUserIsAdmin)
				{
					validationResult.Add(_COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION);
				}
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during native file copy by links validation.";
				_logger.LogError(ex, message);
				validationResult.Add(message);
			}

			return validationResult;
		}

		private async Task<bool> ExecutingUserIsAdminAsync(int workspaceId)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				using (IPermissionManager permissionManager = await _serviceFactory.CreateProxyAsync<IPermissionManager>().ConfigureAwait(false))
				{
					//TODO: Use IGroupManager.QueryGroupsByUserAsync() as will be implemented
					QueryRequest request = BuildAdminGroupsQuery();
					QueryResult queryResult = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);
					RelativityObject adminGroup = queryResult.Objects.Single();

					return await ExecutingUserBelongsToGroupAsync(permissionManager, workspaceId, adminGroup.ArtifactID).ConfigureAwait(false);
				}
			}
		}

		private async Task<bool> ExecutingUserBelongsToGroupAsync(IPermissionManager permissionManager, int workspaceId, int groupId)
		{
			List<UserRef> users = await permissionManager.GetWorkspaceGroupUsersAsync(workspaceId, new GroupRef(groupId)).ConfigureAwait(false);
			return users.Any(x => x.ArtifactID == _userContext.ExecutingUserId);
		}

		private static QueryRequest BuildAdminGroupsQuery()
		{
			const string adminGroupType = "System Admin";
			const int groupArtifactTypeId = 3;
			var request = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = groupArtifactTypeId
				},
				Condition = $"(('Group Type' == '{adminGroupType}'))",
			};

			return request;
		}
	}
}
