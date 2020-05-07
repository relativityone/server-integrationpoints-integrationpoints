﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

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

		public NativeCopyLinksValidator(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger)
		{
			_instanceSettings = instanceSettings;
			_userContext = userContext;
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validating Native File links copy Restriction");

			var validationResult = new ValidationResult();

			try
			{
				if (configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.SetFileLinks)
				{
					return validationResult;
				}

				bool isRestrictReferentialFileLinksOnImport = await _instanceSettings.GetRestrictReferentialFileLinksOnImportAsync().ConfigureAwait(false);
				bool executingUserIsAdmin = await ExecutingUserIsAdminAsync().ConfigureAwait(false);

				_logger.LogInformation("Restrict Referential File Links on Import : {isRestricted}, User is Admin : {isAdmin}",
					isRestrictReferentialFileLinksOnImport, executingUserIsAdmin);

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

		private async Task<bool> ExecutingUserIsAdminAsync()
		{
			_logger.LogInformation("Check if User {userId} is Admin", _userContext.ExecutingUserId);
			using (IGroupManager groupManager = await _serviceFactory.CreateProxyAsync<IGroupManager>().ConfigureAwait(false))
			{
				QueryRequest request = BuildAdminGroupsQuery();
				QueryResultSlim result = await groupManager.QueryGroupsByUserAsync(request, 0, 1, _userContext.ExecutingUserId).ConfigureAwait(false);

				return result.Objects.Any();
			}
		}

		private static QueryRequest BuildAdminGroupsQuery()
		{
			const string adminGroupType = "System Admin";
			var request = new QueryRequest()
			{
				Condition = $"'Group Type' == '{adminGroupType}'",
			};

			return request;
		}
	}
}