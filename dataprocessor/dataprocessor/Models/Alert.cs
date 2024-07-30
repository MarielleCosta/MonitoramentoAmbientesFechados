using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataprocessor.Models
{
    public class Alert
    {
        public DateTime timestamp { get; set; }
        public string message { get; set; }
        public string sensorid { get; set; }

        public Alert(DateTime timestamp, string message, string sensorid)
        {
            this.timestamp = timestamp;
            this.message = message;
            this.sensorid = sensorid;
        }   
        public Alert() { }
    }
}
