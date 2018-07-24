using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace FaultSortApp
{
    /// <summary>
    /// Interaction logic for SortChartWindow.xaml
    /// </summary>
    public partial class SortChartWindow : Window
    {
        public double ConsoleHeight_ { get; set; } = 50;
        ConsoleContent dc = new ConsoleContent();
        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> XFormatter { get; set; }
        public string CellBackgroundStr { get; set; } = "#171717";
        public string CellForegroundStr { get; set; } = "Black";
        //public string[] YLabels { get; set; }
        private DispatcherTimer FetchTimer_;


        public SortChartWindow()
        {
            InitializeComponent();

            consoleItems.ItemsSource = dc.ConsoleOutput;
            dc.AddItemsToConsole("Hello User!");

            SeriesCollection = new SeriesCollection();
            XFormatter = value => String.Format("{0:0.##}", value);

            DataContext = this;
            FetchTimer_ = new DispatcherTimer();
            UpdateFetcherInterval();
            FetchTimer_.Tick += Fetch_Timer_Tick;
            //PlotTestData();
            PlotEngineData(DateTime.Now.AddMilliseconds(-40));
        }

        public void UpdateFetcherInterval()
        {
            FetchTimer_.Interval = TimeSpan.FromMinutes(int.Parse(FetchPeriodMinsInputTB.Text));
        }

        private async void PlotEngineData(DateTime plotTime)
        {
            dc.AddItemsToConsole("Started fetching values from Historian...");
            DateTime startTime = plotTime;
            DateTime endTime = startTime.AddMilliseconds(40);
            FaultSortEngine.FaultSortEngine faultSortEngine = new FaultSortEngine.FaultSortEngine();
            faultSortEngine.GetLinesInfoFromDb();
            List<Tuple<double, string, DateTime>> sortedMaxCurrRatios = await faultSortEngine.GetAllSortedMaxCurrRatios(startTime, endTime);
            dc.AddItemsToConsole("Fetching values from Historian done!");
            List<double> sortedMaxCurrRatioVals = sortedMaxCurrRatios.Select(ratioTuple => ratioTuple.Item1).ToList();
            SeriesCollection.Clear();
            ChartValues<double> chartValues = new ChartValues<double>(sortedMaxCurrRatioVals.Take(20).Reverse());
            string[] chartLabels = sortedMaxCurrRatios.Select(ratioTuple => ratioTuple.Item2).Take(20).Reverse().ToArray();
            MyChart.AxisY[0].Labels = chartLabels;
            SeriesCollection.Add(new RowSeries
            {
                Title = "INeg/IPoss",
                Values = chartValues
            });
            DateTime localTime = TimeZoneInfo.ConvertTime(sortedMaxCurrRatios[0].Item3, TimeZoneInfo.Utc, TimeZoneInfo.Local);
            ChartTitle.Text = $"Sorted I2/I1 ratios at {localTime.ToString()}";
            dc.AddItemsToConsole("Plotting current ratios from Historian done!");
        }

        public void PlotTestData()
        {
            SeriesCollection.Clear();
            SeriesCollection.Add(new RowSeries
            {
                Title = "2016",
                Values = new ChartValues<double> { 11, 56, 42 }
            });
            MyChart.AxisY[0].Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            dc.AddItemsToConsole("Plotting test values done!");
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            dc.ClearConsole();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MyChart.AxisX[0].MinValue = double.NaN;
            MyChart.AxisX[0].MaxValue = double.NaN;
            MyChart.AxisY[0].MinValue = double.NaN;
            MyChart.AxisY[0].MaxValue = double.NaN;
        }

        private void FetchAtTimeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (FetchDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a date...");
                return;
            }
            DateTime plotTime = (DateTime)FetchDatePicker.SelectedDate;
            int hours = int.Parse(HoursInputTB.Text);
            int mins = int.Parse(MinutesInputTB.Text);
            int secs = int.Parse(SecondsInputTB.Text);
            plotTime = new DateTime(plotTime.Year, plotTime.Month, plotTime.Day, hours, mins, secs);
            FetchStopBtn_Click(null, null);
            PlotEngineData(plotTime);
        }

        private void Fetch_Timer_Tick(object sender, EventArgs e)
        {
            PlotEngineData(DateTime.Now.AddMilliseconds(-40));
        }

        private void AutoFetchStart_Click(object sender, RoutedEventArgs e)
        {
            StopFetching();
            dc.AddItemsToConsole("Started Auto Fetching");
            UpdateFetcherInterval();
            FetchTimer_.Start();
            Fetch_Timer_Tick(null, null);
        }

        private void FetchStopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopFetching();
            dc.AddItemsToConsole("Stopped Auto Fetching");
        }

        private void StopFetching()
        {
            // stop the fetch timer if active
            if (FetchTimer_.IsEnabled)
            {
                FetchTimer_.Stop();
            }
        }

    }
}
