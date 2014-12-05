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
		public ActionResult StepDetails2()
		{
			return PartialView("_IntegrationDetailsPartial2");
		}
		public ActionResult Details(int id)
		{
			var integrationViewModel = _reader.ReadIntegrationPoint(id);

			var model = new Models.IpDetailModel();
			model.DataModel = integrationViewModel;
			var grid = ModelFactory.CreateModel("mappedFields", (int) Session["UserID"]);
			grid.colModel = new List<GridColumn>();
			grid.colModel.Add(new GridColumn
			{
				name = "test",
				label = "Workspace Field"
			});

			grid.colModel.Add(new GridColumn
			{
				name = "name",
				label = "Source Attribute"
			});

			grid.JsonReaderOptions = JsonReaderOptions.WebOptions();
			grid.url = Url.Action("GetData", new{id});
			model.Grid = grid;
			
			return View(model);
		}
		
		public JsonNetResult GetData(int id, GridFilterModel filter)
		{
			//TODO: Get this to work
			var result = _reader.GetFieldMap(id);
			return JsonNetResult(result);
		}

		public IEnumerable<object> GetFakeData()
		{
			for (var i = 0; i < 50; i++)
			{
				yield return new
				{
					test = i,
					name = "name" + i

				};
			}

		}

	}
}
