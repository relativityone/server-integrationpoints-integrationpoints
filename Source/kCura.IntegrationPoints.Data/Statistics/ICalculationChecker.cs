using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface ICalculationChecker
    {
        bool IsCalculating(int integrationPointId);

        void MarkAsCalculating(int integrationPointId);

        void MarkCalculationFinished(int integrationPointId);
    }
}
