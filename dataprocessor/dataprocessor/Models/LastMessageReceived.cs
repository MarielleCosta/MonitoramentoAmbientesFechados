using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataprocessor.Models
{
    public class TimeLastMessageReceived
    {
        public string Topic { get; set; }
        public DateTime LastReceived { get; set; }
        public int Interval { get; set; }

        public bool TimoutAlertSent { get; set; } = false;
    }
}
