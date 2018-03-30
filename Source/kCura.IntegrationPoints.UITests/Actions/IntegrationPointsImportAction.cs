﻿using System;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Actions
{
	public class IntegrationPointsImportAction : IntegrationPointsAction
	{
		public IntegrationPointsImportAction(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		protected TImportFirstPage SetupImportFirstPage<TImportFirstPage, TImportSecondPage, TModel>(GeneralPage generalPage, IntegrationPointGeneralModel model,
			Func<TImportFirstPage> funcFirstPageCreator)
			where TImportSecondPage : ImportSecondBasePage<TModel>
			where TImportFirstPage : ImportFirstPage<TImportSecondPage, TModel>
		{
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			TImportFirstPage firstPage = ipPage.CreateNewImportIntegrationPoint<TImportFirstPage, TImportSecondPage, TModel>(funcFirstPageCreator);
			firstPage.Name = model.Name;
			firstPage.SelectImport();
			firstPage.Source = model.SourceProvider;
			firstPage.TransferredObject = model.TransferredObject;
			return firstPage;
		}

		protected TSecondPage SetupImportSecondPage<TSecondPage, TModel>(ImportFirstPage<TSecondPage, TModel> firstPage, TModel model)
			where TSecondPage : ImportSecondBasePage<TModel>
		{
			TSecondPage secondPage = firstPage.GoToNextPage();
			secondPage.SetupModel(model);
			return secondPage;
		}

		protected ImportThirdPage<TModel> SetupImportThirdPage<TModel>(ImportSecondBasePage<TModel> secondPage, TModel model, Func<ImportThirdPage<TModel>> funcThridPageCreator)
		{
			ImportThirdPage<TModel> thirdPage = secondPage.GoToNextPage(funcThridPageCreator);
			return thirdPage;
		}
	}
}
