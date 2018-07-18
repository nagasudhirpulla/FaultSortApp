using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultSortApp.PgFetchAdapter
{
    public class PgDbUtils
    {
        public const string SSLinesDataFetchSQL = @"select min(src.stationname) as station, min(src.devicename) as line, string_agg(concat(src.pointname, '|', src.measurementid), '||') as line_measurements from 
                            (select *
                            from measurement
                            where devicetype = 'L' AND(pointname LIKE 'V%'OR pointname LIKE 'I%')) as src
                            group by(src.devicename, src.stationname)";
    }
}
