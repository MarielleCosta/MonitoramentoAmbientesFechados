using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataprocessor.Models
{
    public class Sensor
    {
        public string sensor_id {  get; set; }
        public string data_type { get; set; }
        public int data_interval { get; set; } // miliseconds

        public void ShowSensor()
        {
            Console.WriteLine("Sensor id: " + sensor_id);
            Console.WriteLine("Data Type: " + data_type);
            Console.WriteLine("interval: " + data_interval.ToString());
        }
    }
}
