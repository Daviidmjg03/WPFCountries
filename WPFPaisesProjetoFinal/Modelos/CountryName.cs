using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPaisesCExecutavel.Modelos
{
    public class CountryName
    {
        public string Common { get; set; }
        public string Official { get; set; }
        public Dictionary<string, CountryNativeName> NativeName { get; set; }
    }
}
