using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface ICalculationChecker
    {
        // Methods to:
        // 1. Get Integration Point RDO field state
        // 2. Integration Point RDO field state update to 'calculation in progress' = true
        // 3. Integration Point RDO field state  update to 'calculation in progress' = false (and set reason: (a) error occurred, (b) statistics value available
    }
}
