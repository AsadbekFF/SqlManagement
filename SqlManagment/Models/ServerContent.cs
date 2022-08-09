using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlManagment.Models
{
    public class ServerContent
    {
        public string ServerName { get; set; }
        public List<DatabaseContent> DatabaseContents { get; set; }
    }
}
