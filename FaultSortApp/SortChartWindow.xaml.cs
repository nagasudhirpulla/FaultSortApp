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


        public SortChartWindow()
        {
            InitializeComponent();

            consoleItems.ItemsSource = dc.ConsoleOutput;
            dc.AddItemsToConsole("Hello User!");

            SeriesCollection = new SeriesCollection();
            XFormatter = value => String.Format("{0:0.##}", value);

            DataContext = this;
            PlotTestData();
            //PlotEngineData();
        }

        private async void PlotEngineData()
        {
            dc.AddItemsToConsole("Started fetching values from Historian...");
            DateTime startTime = DateTime.Now.AddMinutes(-2);
            DateTime endTime = startTime.AddSeconds(1);
            FaultSortEngine.FaultSortEngine faultSortEngine = new FaultSortEngine.FaultSortEngine();
            faultSortEngine.GetLinesInfoFromDb();
            List<Tuple<double, string, DateTime>> sortedMaxCurrRatios = await faultSortEngine.GetAllSortedMaxCurrRatios(startTime, endTime);
            dc.AddItemsToConsole("Fetching values from Historian done!");
            List<double> sortedMaxCurrRatioVals = sortedMaxCurrRatios.Select(ratioTuple => ratioTuple.Item1).ToList();
            SeriesCollection.Clear();
            ChartValues<double> chartValues = new ChartValues<double>(sortedMaxCurrRatioVals.Take(20));
            MyChart.AxisY[0].Labels = sortedMaxCurrRatios.Select(ratioTuple => ratioTuple.Item2).Take(20).ToArray(); ;
            SeriesCollection.Add(new RowSeries
            {
                Title = "INeg/IPoss",
                Values = chartValues
            });            
            dc.AddItemsToConsole("Plotting fault currents from Historian done!");
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
    }
}
