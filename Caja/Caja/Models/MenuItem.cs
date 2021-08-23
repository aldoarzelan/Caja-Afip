using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caja.Models
{
    public class MenuItem
    {
        public string    LinkName { get; set; }
        public string Link { get; set; }
        public int SubNiveles { get; set; } 
        public string Class { get; set; }
        public List<MenuItem> SubMenu { get; set; }
    }
}