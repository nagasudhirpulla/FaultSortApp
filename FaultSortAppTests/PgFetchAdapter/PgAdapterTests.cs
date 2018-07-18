using Microsoft.VisualStudio.TestTools.UnitTesting;
using FaultSortApp.PgFetchAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace FaultSortApp.PgFetchAdapter.Tests
{
    [TestClass()]
    public class PgAdapterTests
    {
        [TestMethod()]
        public void PgAdapterSSLineDatFetchTest()
        {
            // this tests if Substations Line Data fetching is being done without errors
            try
            {
                PgAdapter pgAdapter = new PgAdapter(new PgConfig());
                // sql statement
                string sql = PgDbUtils.SSLinesDataFetchSQL;
                pgAdapter.OpenConn();
                DataTable dt = pgAdapter.GetSQLDataTable(sql);
                pgAdapter.CloseConn();
                Assert.IsTrue(dt.Rows.Count > 0, "Returned zero rows for sql fetch");
            }
            catch (Exception e)
            {
                // something went wrong, and you wanna know why
                Assert.Fail(e.Message);
            }
        }
    }
}