﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.Validation
{
	internal abstract class CopyLinksValidatorBase : IValidator
	{
		private readonly IInstanceSettings _instanceSettings;
		private readonly IUserContextConfiguration _userContext;
		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
		private readonly INonAdminCanSyncUsingLinks _nonAdminCanSyncUsingLinks;
		private readonly IUserService _userService;
		private readonly ISyncLog _logger;

		protected abstract string ValidatorKind { get; }

		protected CopyLinksValidatorBase(IInstanceSettings instanceSettings, IUserContextConfiguration userContext, ISourceServiceFactoryForAdmin serviceFactoryForAdmin, INonAdminCanSyncUsingLinks nonAdminCanSyncUsingLinks, IUserService userService, ISyncLog logger)
		{
			_instanceSettings = instanceSettings;
			_userContext = userContext;
			_serviceFactoryForAdmin = serviceFactoryForAdmin;
			_nonAdminCanSyncUsingLinks = nonAdminCanSyncUsingLinks;
			_userService = userService;
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
				if (ShouldNotValidateReferentialFileLinksRestriction(configuration))
				{
					return validationResult;
				}

				bool isRestrictReferentialFileLinksOnImport = await _instanceSettings.GetRestrictReferentialFileLinksOnImportAsync().ConfigureAwait(false);
				bool executingUserIsAdmin = await _userService.ExecutingUserIsAdminAsync(_userContext).ConfigureAwait(false);

				_logger.LogInformation("Restrict Referential File Links on Import : {isRestricted}, User is Admin : {isAdmin}",
					isRestrictReferentialFileLinksOnImport, executingUserIsAdmin);

				if (isRestrictReferentialFileLinksOnImport && !executingUserIsAdmin && !_nonAdminCanSyncUsingLinks.IsEnabled())
				{
					validationResult.Add(_COPY_NATIVE_FILES_BY_LINKS_LACK_OF_PERMISSION);
				}
			}
			catch (Exception ex)
			{
				string message = $"Exception occurred during {ValidatorKind} copy by links validation.";
				_logger.LogError(ex, message);
				throw;
			}

			return validationResult;
		}

		public abstract bool ShouldValidate(ISyncPipeline pipeline);
		
		protected abstract bool ShouldNotValidateReferentialFileLinksRestriction(IValidationConfiguration configuration);

	}
}