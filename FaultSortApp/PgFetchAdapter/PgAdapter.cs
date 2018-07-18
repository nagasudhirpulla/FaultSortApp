using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace FaultSortApp.PgFetchAdapter
{
    public class PgAdapter
    {
        private NpgsqlConnection PgConnection;
        public PgConfig Config { get; set; }

        public PgAdapter()
        {
            Config = new PgConfig();
        }

        public PgAdapter(PgConfig pgConfig)
        {
            Config = pgConfig;
        }

        private void MakeNewConn()
        {
            PgConnection = new NpgsqlConnection(Config.GetConnString());
        }

        public void MakeValidConn()
        {
            List<ConnectionState> badStates = new List<ConnectionState> { ConnectionState.Broken, ConnectionState.Closed };
            if (PgConnection == null)
            {
                MakeNewConn();
            }
            else if (badStates.Contains(PgConnection.FullState))
            {
                PgConnection.Close();
                PgConnection.Dispose();
                // Making connection with Npgsql provider
                PgConnection = new NpgsqlConnection(Config.GetConnString());
            }
        }

        public NpgsqlConnection GetConn()
        {
            return PgConnection;
        }

        public void OpenConn()
        {
            MakeValidConn();
            if (PgConnection.FullState == ConnectionState.Closed)
            {
                PgConnection.Open();
            }
        }

        public void CloseConn()
        {
            PgConnection.Close();
        }

        public DataTable GetSQLDataTable(string sql)
        {
            return GetSQLDataTable(sql, GetConn());
        }

        public DataTable GetSQLDataTable(string sql, NpgsqlConnection conn)
        {
            // data adapter making request from our connection
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            // i always reset DataSet before i do
            // something with it.... i don't know why :-)
            ds.Reset();
            // filling DataSet with result from NpgsqlDataAdapter
            da.Fill(ds);
            // since it C# DataSet can handle multiple tables, we will select first
            dt = ds.Tables[0];
            // connect grid to DataTable
            return dt;
        }
    }
}
