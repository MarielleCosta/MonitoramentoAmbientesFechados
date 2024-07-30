using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataprocessor.Models
{
    public class InitialMessage
    {
        public string machine_id { get; set; }
        public List<Sensor> sensors { get; set; } = new List<Sensor>();

        public void ShowInitialMessage()
        {
            Console.WriteLine("ShowInitialMessage");
            Console.WriteLine("Machine - ID: " + machine_id);

            foreach (Sensor sensor in sensors)
            {
                sensor.ShowSensor();
            }
        }

    }
}
