using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * Writing data to excel
 * https://stackoverflow.com/questions/23041021/how-to-write-some-data-to-excel-file-xlsx
 * 
 * Find number of elements in a tuple
 * https://stackoverflow.com/questions/7631327/number-of-elements-in-tuple
 */
namespace FaultSortApp.Utils
{
    public class ExcelUtils
    {
        public static void DumpCurrRatioResultsToExcel(List<Tuple<double, string, DateTime>> currRatios, string fName)
        {
            //dump values to excel
            Microsoft.Office.Interop.Excel.Application oXL;
            Microsoft.Office.Interop.Excel._Workbook oWB;
            Microsoft.Office.Interop.Excel._Worksheet oSheet;

            //Start Excel and get Application object.
            oXL = new Microsoft.Office.Interop.Excel.Application();
            oXL.Visible = true;

            //Get a new workbook.
            oWB = (oXL.Workbooks.Add(""));
            oSheet = oWB.ActiveSheet;

            // Iterate through list and dump in excel
            for (int tupleIter = 0; tupleIter < currRatios.Count; tupleIter++)
            {
                oSheet.Cells[tupleIter + 1, 1] = currRatios[tupleIter].Item2;
                oSheet.Cells[tupleIter + 1, 2] = currRatios[tupleIter].Item1;
                oSheet.Cells[tupleIter + 1, 3] = currRatios[tupleIter].Item3;
            }

            oXL.Visible = false;
            oXL.UserControl = false;
            oWB.SaveAs(fName, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            oWB.Close();
        }
    }
}
