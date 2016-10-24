﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
    public interface IFieldParserFactory
    {
        IFieldParser GetFieldParser(string options);
    }
}
