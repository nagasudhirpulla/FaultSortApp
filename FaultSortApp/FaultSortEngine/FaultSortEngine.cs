using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaultSortApp.FetchLogic;
using FaultSortApp.PgFetchAdapter;

namespace FaultSortApp.FaultSortEngine
{
    public class FaultSortEngine
    {
        private PgAdapter PgAdapter = new PgAdapter(new PgConfig());
        private HistoryDataAdapter historyDataAdapter;
        public DataTable LineMeasurementsDataTable { get; set; }

        public FaultSortEngine()
        {
            historyDataAdapter = new HistoryDataAdapter
            {
                _configuration = new ConfigurationManager()
            };
            DoInitialStuff();
        }

        public FaultSortEngine(ConfigurationManager configuration, PgConfig pgConfig)
        {
            historyDataAdapter = new HistoryDataAdapter
            {
                _configuration = configuration
            };
            PgAdapter = new PgAdapter(pgConfig);
            DoInitialStuff();
        }

        public void DoInitialStuff()
        {
            historyDataAdapter._configuration.Initialize();
            historyDataAdapter.Initialize();
        }

        public void GetSSLinesInfo()
        {
            // the Substations Lines Info
            try
            {
                // sql statement
                string sql = PgDbUtils.SSLinesDataFetchSQL;
                PgAdapter.OpenConn();
                DataTable dt = PgAdapter.GetSQLDataTable(sql);
                PgAdapter.CloseConn();
                // check if the column names of the data table are station, line, line_measurements
                List<string> tableColumns = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToList();
                List<string> essentialCols = new List<string> { "station", "line", "line_measurements" };
                bool essentialColsPresent = tableColumns.All(col => essentialCols.Contains(col));
                if (!essentialColsPresent)
                {
                    throw new Exception($"The expeted columns {string.Join(", ", essentialCols)} are not present");
                }

                dt.Columns.Add("meas_dict", typeof(Dictionary<string, string>));

                // get only the rows with line_measurements having keys IBM, IYA, IPM, IRM, IRA, IYM, IBA, IPA
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow dr = dt.Rows[i];
                    // get the line measurements field
                    string measData = dr.Field<string>("line_measurements");
                    IEnumerable<(string, string)> measPairs = measData.Split(new String[] { "||" }, StringSplitOptions.None).ToList().Select(pairStr =>
                     {
                         string[] keyVal = pairStr.Split('|');
                         return (keyVal[0], keyVal[1]);
                     });
                    IEnumerable<string> measKeys = measPairs.ToList().Select(measPair => measPair.Item1);
                    List<string> essentialMeasKeys = "IBM,IYA,IPM,IRM,IRA,IYM,IBA,IPA".Split(new String[] { "," }, StringSplitOptions.None).ToList();
                    bool allMeasKeysPresent = measKeys.All(measKey => essentialMeasKeys.Contains(measKey));
                    if (!allMeasKeysPresent)
                    {
                        dr.Delete();
                        continue;
                    }
                    // create the measurement lookup dictionary for the line
                    dr["meas_dict"] = measPairs.ToDictionary(x => x.Item1, x => x.Item2);
                }
                LineMeasurementsDataTable = dt;
            }
            catch (Exception e)
            {
                // something went wrong, and you wanna know why
                Console.WriteLine(e.Message);
                throw;
            }
        }

    }
}
