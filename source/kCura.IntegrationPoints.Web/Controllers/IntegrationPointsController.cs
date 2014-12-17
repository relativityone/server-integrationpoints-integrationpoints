using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client;
using Newtonsoft.Json;
using Relativity.DragonGrid.Core.Grid;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public class IntegrationPointsController : BaseController
	{
		private readonly IntegrationPointReader _reader;
		private readonly RdoSynchronizer _rdosynchronizer;
		public IntegrationPointsController(IntegrationPointReader reader, RdoSynchronizer rdosynchronizer)
		{
			_rdosynchronizer = rdosynchronizer;
			_reader = reader;
		}

		public ActionResult Edit(int? id)
		{
			return View(id ?? 0);
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


		public JsonNetResult GetSourceFields(string json)
		{


			var list = new List<FieldEntry>()
			{
				new FieldEntry() {DisplayName = "Age", FieldIdentifier = "2"},
				new FieldEntry() {DisplayName = "Database Name", FieldIdentifier = "1"},
				new FieldEntry() {DisplayName = "Date", FieldIdentifier = "4"},
				new FieldEntry() {DisplayName = "Department", FieldIdentifier = "3"},
				new FieldEntry() {DisplayName = "Field", FieldIdentifier = "5"},
			};
			return JsonNetResult(list);
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
