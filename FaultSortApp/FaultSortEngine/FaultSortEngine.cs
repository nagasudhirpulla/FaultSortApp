using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Numerics;
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
        public DateTime WindowStartTime { get; set; } = DateTime.Now.AddHours(-1);
        public DateTime WindowEndTime { get; set; } = DateTime.Now.AddHours(-1).AddMinutes(1);


        public FaultSortEngine()
        {
            historyDataAdapter = new HistoryDataAdapter();
            ConfigurationManager configuration = new ConfigurationManager();
            configuration.Initialize();
            historyDataAdapter.Initialize(configuration);
            DoInitialStuff();
        }

        public FaultSortEngine(ConfigurationManager configuration, PgConfig pgConfig)
        {
            historyDataAdapter = new HistoryDataAdapter();
            PgAdapter = new PgAdapter(pgConfig);
            historyDataAdapter.Initialize(configuration);
            DoInitialStuff();
        }

        public void DoInitialStuff()
        {

        }

        public void GetLinesInfoFromDb()
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
                bool essentialColsPresent = essentialCols.All(col => tableColumns.Contains(col));
                if (!essentialColsPresent)
                {
                    throw new Exception($"The expeted columns {string.Join(", ", essentialCols)} are not present");
                }

                dt.Columns.Add("meas_dict", typeof(Dictionary<string, int>));

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
                    bool allMeasKeysPresent = essentialMeasKeys.All(measKey => measKeys.Contains(measKey));
                    if (!allMeasKeysPresent)
                    {
                        dr.Delete();
                        continue;
                    }
                    // create the measurement lookup dictionary for the line
                    dr["meas_dict"] = measPairs.ToDictionary(x => x.Item1, x => Int32.Parse(x.Item2));
                }
                dt.AcceptChanges();
                LineMeasurementsDataTable = dt;
            }
            catch (Exception e)
            {
                // something went wrong, and you wanna know why
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<Dictionary<object, List<PMUDataStructure>>> GetLineDataAsync(Dictionary<string, int> measDict, DateTime startTime, DateTime endTime)
        {
            //IBM,IYA,IPM,IRM,IRA,IYM,IBA,IPA
            List<int> measIds = measDict.Values.ToList();
            int dataRate = 25;
            Dictionary<object, List<PMUDataStructure>> data = await historyDataAdapter.GetDataAsync(startTime, endTime, measIds, true, false, dataRate);
            return data;
        }

        public async Task<DataTable> GetLineDataTableAsync(Dictionary<string, int> measDict, DateTime startTime, DateTime endTime, bool discardBad = true)
        {
            Dictionary<object, List<PMUDataStructure>> data = await GetLineDataAsync(measDict, startTime, endTime);
            // create a data table based on the rows
            DataTable dt = new DataTable();
            // Add columns to the table
            dt.Columns.Add("timestamp", typeof(DateTime));
            measDict.Keys.ToList().ForEach(key =>
            {
                dt.Columns.Add(key, typeof(float));
                dt.Columns.Add(key + ".quality", typeof(DataQuality));
            });

            if (data == null)
            {
                return dt;
            }

            int RowsCount = data[data.Keys.ElementAt(0)].Count;
            for (int fetchRowIter = 0; fetchRowIter < RowsCount; fetchRowIter++)
            {
                DataRow dr = dt.NewRow();
                bool goodRow = true;
                for (int measKeyIter = 0; measKeyIter < measDict.Keys.Count; measKeyIter++)
                {
                    // iterate through each measurement
                    string measKey = measDict.Keys.ElementAt(measKeyIter);
                    uint measId = (uint)measDict[measKey];
                    PMUDataStructure fetchRow = data[measId][fetchRowIter];
                    //discard bad
                    if (discardBad)
                    {
                        if (fetchRow.Quality != DataQuality.Good && fetchRow.Quality != DataQuality.Replaced)
                        {
                            goodRow = false;
                            break;
                        }
                    }

                    dr[measKey] = fetchRow.Value[0];
                    dr[measKey + ".quality"] = fetchRow.Quality;

                    // get timestamp value
                    if (measKeyIter == 0)
                    {
                        dr["timestamp"] = fetchRow.TimeStamp;
                    }
                }
                if (goodRow)
                {
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        public Tuple<double, DateTime> GetMaxTableCurrentRatio(DataTable dt)
        {
            List<string> columnNames = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToList();
            List<string> essentialColumnNames = "timestamp, IBM, IYA, IPM, IRM, IRA, IYM, IBA, IPA".Split(new string[] { ", " }, StringSplitOptions.None).ToList();
            bool isEssentialColsPresent = essentialColumnNames.All(col => columnNames.Contains(col));
            if (!isEssentialColsPresent)
            {
                throw (new Exception("All required Columns are not present for processing max current ratio"));
            }

            double maxRatio = 0.0;
            DateTime maxRatioTime = new DateTime();

            for (int rowIter = 0; rowIter < dt.Rows.Count; rowIter++)
            {
                DataRow dr = dt.Rows[rowIter];
                float i1Mag = (float)dr["IPM"];
                Complex i2 = Complex.FromPolarCoordinates((float)dr["IRM"], ((float)dr["IRA"]) * Math.PI / 180);
                i2 = Complex.Add(i2, Complex.FromPolarCoordinates((float)dr["IYM"], ((float)dr["IYA"] + 240) * Math.PI / 180));
                i2 = Complex.Add(i2, Complex.FromPolarCoordinates((float)dr["IBM"], ((float)dr["IBA"] + 120) * Math.PI / 180));
                double currentRatio = 3 * i1Mag / i2.Magnitude;
                if (currentRatio > maxRatio)
                {
                    maxRatio = currentRatio;
                    maxRatioTime = (DateTime)dr["timestamp"];
                }
            }
            return new Tuple<double, DateTime>(maxRatio, maxRatioTime);
        }

        public async Task<List<Tuple<double, string, DateTime>>> GetAllMaxCurrRatios(DateTime starttime, DateTime endTime)
        {
            List<Tuple<double, string, DateTime>> maxCurrRatios = new List<Tuple<double, string, DateTime>>();
            for (int lineIter = 0; lineIter < LineMeasurementsDataTable.Rows.Count; lineIter++)
            {
                //Iterate through all the lines
                DataRow dr = LineMeasurementsDataTable.Rows[lineIter];
                Dictionary<string, int> lineMeasInfo = (Dictionary<string, int>)dr["meas_dict"];
                DataTable dt = await GetLineDataTableAsync(lineMeasInfo, starttime, endTime, true);
                Tuple<double, DateTime> maxCurrRatioVal = GetMaxTableCurrentRatio(dt);
                Tuple<double, string, DateTime> maxCurrRatio = new Tuple<double, string, DateTime>(maxCurrRatioVal.Item1, (string)dr["station"], maxCurrRatioVal.Item2);
                maxCurrRatios.Add(maxCurrRatio);
            }
            return maxCurrRatios;
        }

        public async Task<List<Tuple<double, string, DateTime>>> GetAllSortedMaxCurrRatios(DateTime starttime, DateTime endTime)
        {
            List<Tuple<double, string, DateTime>> maxCurrRatios = await GetAllMaxCurrRatios(starttime, endTime);
            maxCurrRatios.Sort((a, b) => (b.Item1.CompareTo(a.Item1)));
            return maxCurrRatios;
        }

    }
}
