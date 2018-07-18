using Microsoft.VisualStudio.TestTools.UnitTesting;
using FaultSortApp.FetchLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultSortApp.FetchLogic.Tests
{
    [TestClass()]
    public class HistoryDataAdapterTests
    {
        [TestMethod()]
        public async Task GetDataTestAsync()
        {
            try
            {
                ConfigurationManager configurationManager = new ConfigurationManager();
                configurationManager.Initialize();
                HistoryDataAdapter historyDataAdapter = new HistoryDataAdapter();
                historyDataAdapter.Initialize(configurationManager);
                // test the data fetch
                DateTime startTime = DateTime.Now.AddHours(-1);
                DateTime endTime = startTime.AddSeconds(5);

                int testKey = 5505;
                int dataRate = 25;
                Dictionary<Object, List<PMUDataStructure>> fetchResp = await historyDataAdapter.GetDataAsync(startTime, endTime, new List<int> { testKey }, true, false, dataRate);

                // check if we got exactly one key
                Assert.AreEqual(fetchResp.Count, 1);

                Assert.AreEqual((uint)fetchResp.Keys.ElementAt(0), (uint)testKey);

                // get the data of the key
                if (!fetchResp.TryGetValue((uint)testKey, out List<PMUDataStructure> measData))
                {
                    // the key isn't in the dictionary.
                    Assert.Fail("Did not get data from the history data adapter dictionary via the measurement key");
                }

                // Test the data list length as per the time span
                Assert.IsTrue(measData.Count == endTime.Subtract(startTime).Seconds * dataRate, "Got unexpected rows for a timespan");

                // check if we are not getting null data
                Assert.IsTrue(measData[0] != null, "Got a null data first row of the measurement");

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}