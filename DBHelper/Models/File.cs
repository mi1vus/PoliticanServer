using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper.Models
{
    public class File
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public int Stage { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public bool IsFile { get; set; }
    }
}
