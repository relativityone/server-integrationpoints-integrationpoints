﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
	internal abstract class CopyLinksValidatorBase : IValidator
	{
		private IInstanceSettings _instanceSettings;
		private IUserContextConfiguration _userContext;
		private ISourceServiceFactoryForAdmin _serviceFactory;
		private ISyncLog _logger;

		protected abstract string ValidatorKind { get; }

		public CopyLinksValidatorBase(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger)
		{
			_instanceSettings = instanceSettings;
			_userContext = userContext;
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		private const string _COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION =
			"You do not have permission to run this import because it uses referential links to files. " +
			"You must either log in as a system administrator or change the settings to upload files to run this import.";

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validating {validatorKind} links copy Restriction", ValidatorKind);

			var validationResult = new ValidationResult();

			try
			{
				if (ShouldSkipValidation(configuration))
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
				string message = $"Exception occurred during {ValidatorKind} copy by links validation.";
				_logger.LogError(ex, message);
				validationResult.Add(message);
			}

			return validationResult;
		}

		public abstract bool ShouldValidate(ISyncPipeline pipeline);


		protected abstract bool ShouldSkipValidation(IValidationConfiguration configuration);

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