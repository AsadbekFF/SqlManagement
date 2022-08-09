using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlManagment.Models
{
    public class ExecuteModel
    {
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string ExecutionText { get; set; }
    }
}
