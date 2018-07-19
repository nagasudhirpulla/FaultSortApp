using Microsoft.VisualStudio.TestTools.UnitTesting;
using FaultSortApp.FaultSortEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

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
                faultSortEngine.GetLinesInfoFromDb();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod()]
        public async Task GetSSDataTableAsyncTest()
        {
            try
            {
                //IBM|17595||IYA|17594||IPM|17597||IRM|17591||IRA|17592||IYM|17593||IBA|17596||IPA|17598
                Dictionary<string, int> testDict = new Dictionary<string, int>();
                testDict.Add("IBM", 17595);
                testDict.Add("IYA", 17594);
                testDict.Add("IPM", 17597);
                testDict.Add("IRM", 17591);
                testDict.Add("IRA", 17592);
                testDict.Add("IYM", 17593);
                testDict.Add("IBA", 17596);
                testDict.Add("IPA", 17598);
                DateTime startTime = DateTime.Now.AddHours(-1);
                DateTime endTime = startTime.AddMinutes(1);
                FaultSortEngine faultSortEngine = new FaultSortEngine();
                DataTable dt = await faultSortEngine.GetLineDataTableAsync(testDict, startTime, endTime, true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }

        [TestMethod()]
        public async Task GetAllSortedMaxCurrRatiosTest()
        {
            try
            {
                DateTime startTime = DateTime.Now.AddHours(-1);
                DateTime endTime = startTime.AddMinutes(1);
                FaultSortEngine faultSortEngine = new FaultSortEngine();
                faultSortEngine.GetLinesInfoFromDb();
                List<Tuple<double, string, DateTime>> sortedMaxCurrRatios = await faultSortEngine.GetAllSortedMaxCurrRatios(faultSortEngine.WindowStartTime, faultSortEngine.WindowEndTime);
                List<double> sortedMaxCurrRatioVals = sortedMaxCurrRatios.Select(ratioTuple => ratioTuple.Item1).ToList();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}