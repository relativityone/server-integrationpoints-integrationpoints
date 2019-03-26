using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{
	[TestFixture]
	[Category(TestCategory.EXPORT_TO_LOAD_FILE)]
	[Category(TestCategory.BROKEN_ON_REGRESSION_ENVIRONMENT)] // REL-294344
	public class EntityExportToLoadFile : UiTest
	{
		private IntegrationPointsAction _integrationPointsAction;
		private IRSAPIService _rsapiService;

		private const string _VIEW_NAME = "RIP_TC_ELF_CUST_UI_TEST";

		private IRelativityObjectManager ObjectManager => _rsapiService.RelativityObjectManager;

		[OneTimeSetUp]
		public async Task OneTimeSetup()
		{
			Context.AddEntityObjectToWorkspace();
			await Context.CreateEntityView(_VIEW_NAME);

			Install(Context.WorkspaceId.Value);
			_rsapiService = Container.Resolve<IRSAPIService>();

			SetupEntities();
		}

		[SetUp]
		public void Setup()
		{
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}

		[Test]
		[RetryOnError]
		public void EntityExportToLoadFile_TC_ELF_CUST_1()
		{
			var model = new EntityExportToLoadFileModel("TC-ELF-CUST-1");
			model.TransferredObject = TransferredObjectConstants.ENTITY;
			model.ExportDetails = new EntityExportToLoadFileDetails
			{
				View = _VIEW_NAME,
				SelectAllFields = true,
				ExportTextFieldsAsFiles = true,
				DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root
			};

			model.OutputSettings = new ExportToLoadFileOutputSettingsModel
			{
				LoadFileOptions = new ExportToLoadFileLoadFileOptionsModel
				{
					DataFileFormat = "Comma-separated (.csv)",
					DataFileEncoding = "Unicode",
					ExportMultiChoiceAsNested = true,
					AppendOriginalFileName = true,
				},
				TextOptions = new ExportToLoadFileTextOptionsModel
				{
					TextFileEncoding = "Unicode (UTF-8)",
					TextPrecedence = "Notes",
					TextSubdirectoryPrefix = "TEXT",
				},
				VolumeAndSubdirectoryOptions = new ExportToLoadFileVolumeAndSubdirectoryModel
				{
					VolumePrefix = "VOL",
					VolumeStartNumber = 1,
					VolumeNumberOfDigits = 4,
					VolumeMaxSize = 4400,
					SubdirectoryStartNumber = 1,
					SubdirectoryNumberOfDigits = 4,
					SubdirectoryMaxFiles = 500
				}
			};
			var validator = new ExportToLoadFileProviderValidator();

			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportEntityToLoadfileIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			validator.ValidateTransferedItems(detailsPage, 1);
		}

		private void SetupEntities()
		{
			var objectTypeReference = new ObjectTypeRef
			{
				Guid = Core.Contracts.Entity.ObjectTypeGuids.Entity
			};

			var fieldValues = new List<FieldRefValuePair>
			{
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = Guid.Parse(EntityFieldGuids.FirstName)},
					Value = "Grzegorz"
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = Guid.Parse(EntityFieldGuids.LastName)},
					Value = "Brzeczyszczykiewicz"
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = Guid.Parse(EntityFieldGuids.Email)},
					Value = "Grzegorz.Brzeczyszczykiewicz@company.com"
				}
			};

			ObjectManager.Create(objectTypeReference, fieldValues);
		}
	}
}
