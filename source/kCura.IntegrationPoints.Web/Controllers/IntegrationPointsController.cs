using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
		private readonly IntegrationPointReader _reader;
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

		public ActionResult StepDetails3()
		{
			return PartialView("_IntegrationDetailsPartial3");
		}

		public ActionResult ConfigurationDetail()
		{
			return PartialView("_Configuration");
		}

		public ActionResult LDAPConfiguration()
		{
			return View("LDAPConfiguration", "_StepLayout");
		}

		public ActionResult Details(int id)
		{
			var integrationViewModel = _reader.ReadIntegrationPoint(id);

			var model = new Models.IpDetailModel();
			model.DataModel = integrationViewModel;

			return View(model);
		}

		public JsonNetResult GetGridModel(int id)
		{
			var grid = ModelFactory.CreateModel("mapFieldsGrid", (int)Session["UserID"]);
			grid.colModel = new List<GridColumn>();
			grid.colModel.Add(new GridColumn
			{
				name = "workspace",
				label = "Workspace Field"
			});

			grid.colModel.Add(new GridColumn
			{
				name = "source",
				label = "Source Attribute"
			});

			grid.JsonReaderOptions = JsonReaderOptions.WebOptions();
			grid.url = Url.Action("GetData", new { id });
			return JsonNetResult(grid);
		}

		public ActionResult CheckLdap(object model)
		{
			return base.JsonNetResult("error");
		}

		public JsonResult GetWorkspaceFields()
		{
			return Json("[{'name':'jame','identifier':'1'},{'name':'jame','identifier':'1'},{'name':'jame','identifier':'1'}]");
		}

		public JsonResult getSourcefields()
		{
			return null;
		}

		public JsonNetResult GetData(int id, GridFilterModel filter)
		{
			//TODO: Get this to work
			var result = _reader.GetFieldMap(id);
			var mappings = result.Select(x => new { workspace = x.DestinationField.DisplayName, source = x.SourceField.DisplayName });
			var data = new GridData();
			data.BindData(mappings, filter);
			return JsonNetResult(data);
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
