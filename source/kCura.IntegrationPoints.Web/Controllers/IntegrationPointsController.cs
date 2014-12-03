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

			var model = new Models.IpDetailModel();
			model.DataModel = integrationViewModel;
			return View(model);


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

		public JsonNetResult GetData(global::Relativity.DragonGrid.Core.Grid.GridFilterModel filter)
		{
			var gridData = new global::Relativity.DragonGrid.Core.Grid.GridData();
			gridData.BindData(GetFakeData().ToList(), filter);
			return JsonNetResult(gridData);
		}


		public IEnumerable<object> GetFakeData()
		{
			for (var i = 0; i < 12; i++)
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
