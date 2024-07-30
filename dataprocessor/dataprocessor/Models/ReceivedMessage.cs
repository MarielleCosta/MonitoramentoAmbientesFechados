using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataprocessor.Models
{
    public class ReceivedMessage
    {
        public DateTime timestamp { get; set; }
        public double value { get; set; }
        public string sensorid { get; set; }

        public void Show()
        {
            Console.WriteLine("Sensor Id: " + sensorid +
                              " Timestamp: " + timestamp.ToString() + 
                              " Value: " +  value.ToString());
        }
    }
}
