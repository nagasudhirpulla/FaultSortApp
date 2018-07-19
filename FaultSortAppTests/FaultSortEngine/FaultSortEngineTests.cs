using Microsoft.VisualStudio.TestTools.UnitTesting;
using FaultSortApp.FaultSortEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultSortApp.FaultSortEngine.Tests
{
    [TestClass()]
    public class FaultSortEngineTests
    {
        [TestMethod()]
        public void GetSSLinesInfoTest()
        {
            try
            {
                FaultSortEngine faultSortEngine = new FaultSortEngine();
                faultSortEngine.GetSSLinesInfo();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}