using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile
{
	[TestFixture]
	public class EntityExportToLoadFile : UiTest
	{
		private IntegrationPointsAction _integrationPointsAction;
		private IRSAPIService _service;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			EnsureGeneralPageIsOpened();
			Context.InstallLegalHold();
			Install(Context.WorkspaceId.Value);
			_service = Container.Resolve<IRSAPIService>();

			SetupEntities();

			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);

		}

		[Test]
		public void EntityExportToLoadFile_TC_ELF_CUST_1()
		{
			EntityExportToLoadFileModel model = new EntityExportToLoadFileModel("TC-ELF-CUST-1");
			model.TransferredObject = "Custodian";
			model.ExportDetails = new EntityExportToLoadFileDetails
			{
				View = "Custodians - Legal Hold View"
			};
			model.ExportDetails.SelectAllFields = true;
			model.ExportDetails.ExportTextFieldsAsFiles = true;
			model.ExportDetails.DestinationFolder = ExportToLoadFileProviderModel.DestinationFolderTypeEnum.Root;

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
				Guid = Guid.Parse(@"d216472d-a1aa-4965-8b36-367d43d4e64c")
			},
				new List<FieldRefValuePair>()
				{
					new FieldRefValuePair()
					{
						Field = new FieldRef {Guid = Guid.Parse(@"34ee9d29-44bd-4fc5-8ff1-4335a826a07d")},
						Value = "First name"
					},
					new FieldRefValuePair()
					{
						Field = new FieldRef {Guid = Guid.Parse(@"0b846e7a-6e05-4544-b5a8-ad78c49d0257")},
						Value = "Last name"
					}
				});
		}
	}
}
