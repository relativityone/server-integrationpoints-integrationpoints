﻿using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
    public class FileshareController : BaseController
    {
        public ActionResult SavedSearchPicker()
        {
            return PartialView();
        }

        public ActionResult ExportDetails()
        {
            return PartialView();
        }

        public ActionResult TextPrecedencePicker()
        {
            return PartialView();
        }
    }
}