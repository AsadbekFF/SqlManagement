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
        private static LoginPage ServerContent = null;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        #region Login page
        public IActionResult Login(LoginPage login)
        {
            ServerContent serverContent = new();

            try
            {
                using (SqlConnection con = new(@$"Server={login.Server};User Id={login.LoginName};Password={login.Password};"))
                {
                    con.Open();
                    using (SqlCommand cmd = new("SELECT name from sys.databases", con))
                    {
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                dr.Read();
                            }
                            while (dr.Read())
                            {
                                dbNames.Add(dr[0].ToString());
                            }
                        }
                    }
                    ServerContent = login;

                }
            }
            catch (Exception)
            {
                return Error();
            }
            serverContent = GetAllDataFromServer(ServerContent);

            ViewData["ServerInfo"] = serverContent;
            ViewData["Data"] = null;
            return View("Database");
        }
        #endregion

        #region Retrieving all databases, tables, columns names from the server
        private static ServerContent GetAllDataFromServer(LoginPage serverInfo)
        {
            List<DatabaseContent> databaseContents = new();
            ServerContent serverContent;
            foreach (var item in dbNames)
            {
                List<DatabaseColumnsNames> columnsNames = new();
                List<string> tables = new();
                SqlConnection connection = new(@$"Server={serverInfo.Server};Database={item};User Id={serverInfo.LoginName};Password={serverInfo.Password};");
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
                ServerName = serverInfo.Server
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
                SqlConnection connection = new(@$"Server={ServerContent.Server};Database={executeModel.DatabaseName};User Id={ServerContent.LoginName};Password={ServerContent.Password};");
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
            ViewData["ServerInfo"] = GetAllDataFromServer(ServerContent);
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
