using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlManagment.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlManagment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static readonly List<string> dbNames = new();
        private static string ServerName = null;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        #region Login page
        public IActionResult Login(LoginPage login)
        {
            ServerContent serverContent = new();
            
            if (login.LoginName == "Asadbek" && login.Password == "Asadbek2202")
            {
                try
                {
                    using (SqlConnection con = new(@$"Server={login.Server};Trusted_Connection=True;"))
                    {
                        con.Open();
                        ServerName = login.Server;
                        using (SqlCommand cmd = new("SELECT name from sys.databases", con))
                        {
                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    dr.Read();
                                }
                                while (dr.Read())
                                {
                                    dbNames.Add(dr[0].ToString());
                                }
                            }
                        }

                    }
                }
                catch (Exception)
                {
                    return Error();
                }
                serverContent = GetAllDataFromServer(login.Server);

                ViewData["ServerInfo"] = serverContent;
                ViewData["Data"] = null;
                return View("Database");
            }
            else
            {
                return View("Index");
            }
        }
        #endregion

        #region Retrieving all databases, tables, columns names from the server
        private static ServerContent GetAllDataFromServer(string loginServerName)
        {
            List<DatabaseContent> databaseContents = new();
            ServerContent serverContent;
            foreach (var item in dbNames)
            {
                List<DatabaseColumnsNames> columnsNames = new();
                List<string> tables = new();
                SqlConnection connection = new(@$"Server={loginServerName};Database={item};Trusted_Connection=True;");
                connection.Open();
                using SqlCommand cmd = new($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", connection);
                using (SqlDataReader dataReader = cmd.ExecuteReader())
                {

                    while (dataReader.Read())
                    {
                        tables.Add(dataReader[0].ToString());
                    }
                }

                foreach (var table in tables)
                {
                    List<string> columns = new();
                    using SqlDataReader sqlDataReader = new SqlCommand($"SELECT COLUMN_NAME,* FROM INFORMATION_SCHEMA.COLUMNS " +
                            $"WHERE TABLE_NAME = '{table}' AND TABLE_SCHEMA = 'dbo'", connection).ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        columns.Add(sqlDataReader[0].ToString());
                    }
                    DatabaseColumnsNames databaseColumnsNames = new()
                    {
                        TableName = table,
                        TableColumnsNames = columns
                    };

                    columnsNames.Add(databaseColumnsNames);
                }

                databaseContents.Add(new DatabaseContent() { Name = item, DatabaseTableNames = columnsNames });
            }

            serverContent = new()
            {
                DatabaseContents = databaseContents,
                ServerName = loginServerName
            };
            return serverContent;
        }
        #endregion

        #region Execution of entered text by user
        public IActionResult Execute(ExecuteModel executeModel)
        {
            List<string> columnNames = new();
            List<List<string>> data = new();
            
            string ellapsedTime = null;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            try
            {
                SqlConnection connection = new(@$"Server={ServerName};Database={executeModel.DatabaseName};Trusted_Connection=True;");
                connection.Open();
                using SqlDataReader sqlDataReader = new SqlCommand($"SELECT COLUMN_NAME,* FROM INFORMATION_SCHEMA.COLUMNS " +
                            $"WHERE TABLE_NAME = '{executeModel.TableName}' AND TABLE_SCHEMA = 'dbo'", connection).ExecuteReader();
                while (sqlDataReader.Read())
                {
                    columnNames.Add(sqlDataReader[0].ToString());
                }

                data.Add(columnNames);
                sqlDataReader.Close();
                SqlCommand sqlCommand = new($"{executeModel.ExecutionText}", connection);
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                while (dataReader.Read())
                {
                    List<string> vs = new();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        vs.Add(dataReader[i].ToString());
                    }
                    data.Add(vs);
                }

                ViewData["Status"] = "Query executed successfully";
            }
            catch (Exception)
            {
                ViewData["Status"] = "Query has not been executed";
            }

            stopwatch.Stop();
            ellapsedTime = stopwatch.ElapsedMilliseconds.ToString();
            ViewData["EllapsedTime"] = ellapsedTime;
            ViewData["Data"] = data;
            ViewData["ServerInfo"] = GetAllDataFromServer(ServerName);
            return View("Database");
        }
        #endregion
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
