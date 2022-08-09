using System.Collections.Generic;

namespace SqlManagment.Models
{
    public class DatabaseContent
    {
        public string Name { get; set; }
        public List<DatabaseColumnsNames> DatabaseTableNames { get; set; }
    }
}