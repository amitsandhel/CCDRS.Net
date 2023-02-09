using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCDRSManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCDRSManager.Tests
{
    [TestClass()]
    public class CCDRSManagerModelRepositoryTests
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyInitialize]
        public void StartUp()
        {
            Configuration.Initialize();
        }

        [TestMethod()]
        public void TryGetVehicleTest()
        {
            Assert.Fail();
        }
    }
}