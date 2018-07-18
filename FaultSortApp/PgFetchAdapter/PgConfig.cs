using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace FaultSortApp.PgFetchAdapter
{
    public class PgConfig
    {
        public string Dbname { get; set; } = "application_config";
        public string Username { get; set; } = "postgres";
        public string Hostname { get; set; } = "172.16.183.131";
        public string HostPort { get; set; } = "5432";
        public string Password { get; set; } = "P0stgres";

        public PgConfig()
        {

        }

        public PgConfig(string dbname, string username, string hostname, string hostPort, string password)
        {
            Dbname = dbname;
            Username = username;
            Hostname = hostname;
            HostPort = hostPort;
            Password = password;
        }

        public string GetConnString()
        {
            string connstring = String.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};SSL Mode=Prefer;",
                    Hostname, HostPort, Username,
                    Password, Dbname);
            return connstring;
        }
    }
}
