using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{
	[TestFixture]
	[Category(TestCategory.EXPORT_TO_LOAD_FILE)]
	public class EntityExportToLoadFile : UiTest
	{
		private IntegrationPointsAction _integrationPointsAction;
		private IRSAPIService _service;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			Context.InstallLegalHold();
			Install(Context.WorkspaceId.Value);
			_service = Container.Resolve<IRSAPIService>();

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
			EntityExportToLoadFileModel model = new EntityExportToLoadFileModel("TC-ELF-CUST-1");
			model.TransferredObject = TransferredObjectConstants.ENTITY;
			model.ExportDetails = new EntityExportToLoadFileDetails
			{
				View = "Entity - Legal Hold View",
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
			_service.RelativityObjectManager.Create(new ObjectTypeRef()
			{
				Guid = Core.Contracts.Entity.ObjectTypeGuids.Entity
			},
				new List<FieldRefValuePair>()
				{
					new FieldRefValuePair()
					{
						Field = new FieldRef {Guid = Guid.Parse(EntityFieldGuids.FirstName)},
						Value = "Grzegorz"
					},
					new FieldRefValuePair()
					{
						Field = new FieldRef {Guid = Guid.Parse(EntityFieldGuids.LastName)},
						Value = "Brzeczyszczykiewicz"
					},
					new FieldRefValuePair()
					{
						Field = new FieldRef {Guid = Guid.Parse(EntityFieldGuids.Email)},
						Value = "Grzegorz.Brzeczyszczykiewicz@company.com"
					}
				});
		}
	}
}
