using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;
using FaultSortApp.FetchLogic;
using FaultSortApp.PgFetchAdapter;
/*
 https://www.codeproject.com/Articles/30989/Using-PostgreSQL-in-your-C-NET-application-An-intr
     */
namespace FaultSortApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConfigManager configurationManager;
        private HistoryDataAdapter historyDataAdapter;

        public MainWindow()
        {
            InitializeComponent();
            configurationManager = new ConfigManager();
            configurationManager.Initialize();
            historyDataAdapter = new HistoryDataAdapter();
            historyDataAdapter.Initialize(configurationManager);
        }

        private async void FetchTestBtn_Click(object sender, RoutedEventArgs e)
        {
            // test the data fetch
            DateTime startTime = DateTime.Now.AddHours(-1);
            DateTime endTime = startTime.AddSeconds(5);

            Dictionary<Object, List<PMUDataStructure>> fetchResp = await historyDataAdapter.GetDataAsync(startTime, endTime, new List<int> { 5505 }, true, false, 25);
        }

        private void FetchPostgresBtn_Click(object sender, RoutedEventArgs e)
        {
            TestPgAdapter();
        }

        private void TestDirectPgExec()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                // PostgeSQL-style connection string
                string dbname = "application_config";
                string username = "postgres";
                string hostname = "172.16.183.131";
                string hostPort = "5432";
                string password = "P0stgres";
                string connstring = String.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};SSL Mode=Prefer;",
                    hostname, hostPort, username,
                    password, dbname);

                // Making connection with Npgsql provider
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();

                // sql statement
                string sql = @"select min(src.stationname) as station, min(src.devicename) as line, string_agg(concat(src.pointname, '|', src.measurementid), '||') as line_measurements from 
                            (select *
                            from measurement
                            where devicetype = 'L' AND(pointname LIKE 'V%'OR pointname LIKE 'I%')) as src
                            group by(src.devicename, src.stationname)";

                // data adapter making request from our connection
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                // i always reset DataSet before i do
                // something with it.... i don't know why :-)
                ds.Reset();
                // filling DataSet with result from NpgsqlDataAdapter
                da.Fill(ds);
                // since it C# DataSet can handle multiple tables, we will select first
                dt = ds.Tables[0];
                // connect grid to DataTable
                ResDataGridView.DataContext = dt;
                // since we only showing the result we don't need connection anymore
                conn.Close();
            }
            catch (Exception msg)
            {
                // something went wrong, and you wanna know why
                MessageBox.Show(msg.ToString());
                throw;
            }
        }

        private void TestPgAdapter()
        {
            try
            {
                PgAdapter pgAdapter = new PgAdapter(new PgConfig());
                // sql statement
                string sql = PgDbUtils.SSLinesDataFetchSQL;
                pgAdapter.OpenConn();
                DataTable dt = pgAdapter.GetSQLDataTable(sql);
                ResDataGridView.DataContext = dt;
                pgAdapter.CloseConn();
            }
            catch (Exception msg)
            {
                // something went wrong, and you wanna know why
                MessageBox.Show(msg.ToString());
                throw;
            }
        }
    }
}
