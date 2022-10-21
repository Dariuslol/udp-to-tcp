using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Client.Models.Enums;

namespace Client.Models
{
    public class HelloMSG
    {
        public Messages Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int ConID { get; set; }
        
    }
}
