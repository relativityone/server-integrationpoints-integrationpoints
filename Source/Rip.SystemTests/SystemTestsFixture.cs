using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rip.SystemTests
{
    [SetUpFixture]
    public class SystemTestsFixture
    {
        [OneTimeSetUp]
        public void InitializeFixture()
        {

        }

        [OneTimeTearDown]
        public void TearDownFixture()
        {

        }

    }
}
