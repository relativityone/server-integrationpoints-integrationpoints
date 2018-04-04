using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Validation
{
	using System;
	using System.Collections.Generic;
	using Components;
	using IntegrationPoint.Tests.Core.Extensions;

	public class ExportToLoadFileProviderValidator : BaseUiValidator
	{
		public void ValidateSummaryPage(IntegrationPointDetailsPage integrationPointDetailsPage, ExportToLoadFileProviderModel integrationPointModel, bool expectHasErrors, int? transferedItems = null)
		{
			Dictionary<string, string> generalPropertiesTable = integrationPointDetailsPage.SelectGeneralPropertiesTable().Properties;

			ValidateHasErrorsProperty(generalPropertiesTable, expectHasErrors);

			ValidateGeneralModel(generalPropertiesTable, integrationPointModel);
		}
        
	    private static void ValidateHasErrorsProperty(Dictionary<string, string> generalPropertiesTable, bool expectHasErrors)
		{
			Assert.AreEqual(expectHasErrors.AsHtmlString(), generalPropertiesTable["Has Errors:"]);
		}

        
		private static void ValidateGeneralModel(Dictionary<string, string> generalPropertiesTable, ExportToLoadFileProviderModel integrationPointModel)
		{
			Assert.AreEqual(integrationPointModel.GetValueOrDefault(model => model.Name), generalPropertiesTable["Name:"]);
			StringAssert.AreEqualIgnoringCase(integrationPointModel.GetValueOrDefault(model => model.DestinationProvider), generalPropertiesTable["Export Type:"]);
			Assert.AreEqual(integrationPointModel.GetValueOrDefault(model => model.LogErrors).AsHtmlString(), generalPropertiesTable["Log Errors:"]);
			Assert.AreEqual(integrationPointModel.GetValueOrDefault(model => model.EmailNotifications), generalPropertiesTable["Email Notification Recipients:"]);
			Assert.AreEqual(integrationPointModel.GetValueOrDefault(model => model.IncludeInEcaPromote).AsHtmlString(), generalPropertiesTable["Included in ECA Promote List:"]);

			StringAssert.StartsWith("FileShare", generalPropertiesTable["Destination details:"]);   //TODO: check destination folder

			ValidateSourceDetailsModel(generalPropertiesTable, integrationPointModel.SourceInformationModel);
			ValidateFileDetailsModel(generalPropertiesTable, integrationPointModel.ExportDetails);
			ValidateOutputSettingsModel(generalPropertiesTable, integrationPointModel.OutputSettings);
		}

		private static void ValidateSourceDetailsModel(Dictionary<string, string> generalPropertiesTable, ExportToLoadFileSourceInformationModel sourceInformationModel)
		{
			string expectedSourceDetails = GetExpectedSourceDetails(sourceInformationModel);

			Assert.AreEqual(sourceInformationModel.GetValueOrDefault(model => model.StartAtRecord).AsHtmlString(), generalPropertiesTable["Start at record:"]);
			StringAssert.AreEqualIgnoringCase(expectedSourceDetails, generalPropertiesTable["Source details:"]);
		}

		private static void ValidateFileDetailsModel(Dictionary<string, string> generalPropertiesTable, ExportToLoadFileDetailsModel fileDetailsModel)
		{
			Assert.AreEqual(fileDetailsModel.GetValueOrDefault(model => model.OverwriteFiles).AsHtmlString(), generalPropertiesTable["Overwrite files:"]);
		}

		private static void ValidateOutputSettingsModel(Dictionary<string, string> generalPropertiesTable, ExportToLoadFileOutputSettingsModel outputSettingsModel)
		{
			ValidateLoadFileOptionsModel(generalPropertiesTable, outputSettingsModel.LoadFileOptions);
		}

		private static void ValidateLoadFileOptionsModel(Dictionary<string, string> generalPropertiesTable, ExportToLoadFileLoadFileOptionsModel loadFileOptionsModel)
		{
			string expectedFilePath = GetExpectedFilePath(loadFileOptionsModel);
			string expectedLoadFileFormat = GetExpectedFileFormat(loadFileOptionsModel);
			string expectedTextAndNatives = GetExpectedTextAndNativeFileNames(loadFileOptionsModel);

			Assert.AreEqual(expectedLoadFileFormat, generalPropertiesTable["Load file format:"]);
			Assert.AreEqual(expectedTextAndNatives, generalPropertiesTable["Text and Native File Names:"]);
			Assert.AreEqual(expectedFilePath, generalPropertiesTable["File path:"]);

			Assert.AreEqual(loadFileOptionsModel.GetValueOrDefault(model => model.ExportMultiChoiceAsNested).AsHtmlString(), generalPropertiesTable["Multiple choice as nested:"]);
		}

		private static string GetExpectedSourceDetails(ExportToLoadFileSourceInformationModel sourceInformationModel)
		{
			string sourceType = sourceInformationModel.GetValueOrDefault(model => model.Source);
			string savedSearchName = sourceInformationModel.GetValueOrDefault(model => model.SavedSearch);	//TODO: only supports saved searches

			return $"{sourceType}: {savedSearchName}";
		}

		private static string GetExpectedFileFormat(ExportToLoadFileLoadFileOptionsModel loadFileOptionsModel)
		{
			string fileFormat = loadFileOptionsModel.GetValueOrDefault(model => model.DataFileFormat);
			string fileEncoding = loadFileOptionsModel.GetValueOrDefault(model => model.DataFileEncoding);

			return $"{fileFormat}; {fileEncoding}";
		}

		private static string GetExpectedTextAndNativeFileNames(ExportToLoadFileLoadFileOptionsModel loadFileOptionsModel)
		{
			string nameFilesAfter = loadFileOptionsModel.GetValueOrDefault(model => model.NameOutputFilesAfter);

			return $"Named after: {nameFilesAfter}";
		}

		private static string GetExpectedFilePath(ExportToLoadFileLoadFileOptionsModel loadFileOptionsModel)
		{
			string includeNatives = loadFileOptionsModel.GetValueOrDefault(model => model.IncludeNativeFilesPath) ? "Include" : "Do not include";
			string filePathType = loadFileOptionsModel.GetValueOrDefault(model => model.FilePathType).ToString();//TODO: should support all options

			return $"{includeNatives}; {filePathType}";
		}

	    public void ValidateTransferedItems(IntegrationPointDetailsPage detailsPage, int transferedItems)
	    {
	        var history = detailsPage.GetLatestJobHistoryFromJobStatusTable();
            Assert.AreEqual(history.ItemsTransferred, history.TotalItems);
	        Assert.AreEqual(transferedItems, history.ItemsTransferred);
	        Assert.AreEqual(0, history.ItemsWithErrors);
	    }
	}
}
