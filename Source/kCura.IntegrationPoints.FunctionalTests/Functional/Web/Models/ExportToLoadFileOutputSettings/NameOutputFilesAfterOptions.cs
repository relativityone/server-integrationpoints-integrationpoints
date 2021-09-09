using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings
{
    public enum NameOutputFilesAfterOptions
    {
        [Term("Select...")]
        Select,
        Identifier,
        [Term("Begin production number")]
        BeginProductionNumber
    }
}
