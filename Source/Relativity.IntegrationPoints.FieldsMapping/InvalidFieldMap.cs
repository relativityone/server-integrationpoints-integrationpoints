using Relativity.IntegrationPoints.FieldsMapping.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public class InvalidFieldMap
    {
        public FieldMap FieldMap { get; set; }

        public IList<string> InvalidReasons { get; set; } = new List<string>();
    }
}
