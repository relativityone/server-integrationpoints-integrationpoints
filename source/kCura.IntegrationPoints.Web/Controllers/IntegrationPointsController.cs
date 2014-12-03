using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using Relativity.DragonGrid.Core.Grid;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : BaseController
	{
		private IntegrationPointReader _reader;
		public IntegrationPointsController(IntegrationPointReader reader)
		{
			_reader = reader;
		}

	
		public ActionResult Edit(int? objectID)
		{
			return View();
		}
		
		public ActionResult StepDetails()
		{
			return PartialView("_IntegrationDetailsPartial");
		}

		public ActionResult Details(int id)
		{
			var integrationViewModel = _reader.ReadIntegrationPoint(id);
			return View(integrationViewModel);
		}

		public ActionResult Test()
		{
			var grid = new global::Relativity.DragonGrid.Core.Grid.GridModel("mappedFields");
			grid.colModel =new List<GridColumn>();
			grid.colModel.Add(new GridColumn
			{
				name = "test"
			});

			grid.colModel.Add(new GridColumn
			{
				name = "name"
			});

			grid.JsonReaderOptions = JsonReaderOptions.WebOptions();
			grid.url = Url.Action("GetData");
			return View(grid);
		}

		public JsonNetResult GetData()
		{
			var gridData = new global::Relativity.DragonGrid.Core.Grid.GridData();
			gridData.BindData(GetFakeData(),null);

			return JsonNetResult(GetFakeData());
		}


		public IEnumerable<object> GetFakeData()
		{
			for (var i = 0; i < 100; i++)
			{
				yield return new
				{
					test=i,
					name= "name"+i

				};
			}

		} 

	}
}
