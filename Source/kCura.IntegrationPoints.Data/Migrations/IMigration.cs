using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Migrations
{
    public interface IMigration
    {
        void Execute();
    }
}
