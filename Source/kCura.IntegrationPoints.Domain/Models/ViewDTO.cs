using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ViewDTO  : BaseDTO
    {
        public bool IsAvailableInObjectTab { get; set; }

        public int? Order { get; set; }
    }
}
