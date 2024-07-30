using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataprocessor.Models
{
    public class Limites
    {
        public double TempMax { get; set; }
        public double TempMin { get; set; }
        public double HumiMax { get; set; }
        public double HumiMin { get; set; }
        public double LumiMax { get; set; }
        public double LumiMin { get; set; }

        public Limites()
        {
            string baseDirectory = Directory.GetCurrentDirectory();
            string AppSettingsDirectory = Directory.GetParent(baseDirectory).Parent.Parent.FullName;            

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppSettingsDirectory)
                .AddJsonFile("appsettings.json", optional: false);
            
            IConfiguration config = builder.Build();

            TempMax = Convert.ToDouble(config.GetSection("Limites").GetSection("Temperatura").GetSection("Maximo").Value);
            TempMin = Convert.ToDouble(config.GetSection("Limites").GetSection("Temperatura").GetSection("Minimo").Value);
            HumiMax = Convert.ToDouble(config.GetSection("Limites").GetSection("Humidade").GetSection("Maximo").Value);
            HumiMin = Convert.ToDouble(config.GetSection("Limites").GetSection("Humidade").GetSection("Minimo").Value);
            LumiMax = Convert.ToDouble(config.GetSection("Limites").GetSection("Luminosidade").GetSection("Maximo").Value);
            LumiMin = Convert.ToDouble(config.GetSection("Limites").GetSection("Luminosidade").GetSection("Minimo").Value);
        }

        public string VerifyLimits(ReceivedMessage message)
        {
            string result = "";

            if (message.sensorid.ToUpper().Contains("TEMPERATURE"))
            {
                if(message.value > TempMax)
                {
                    return message.sensorid + " Maior que a temperatura MÁXIMA";
                }
                if (message.value < TempMin)
                {
                    return message.sensorid + " Menor que a temperatura MINIMA";
                }
            }

            if (message.sensorid.ToUpper().Contains("HUMIDITY"))
            {
                if (message.value > HumiMax)
                {
                    return message.sensorid + " Maior que a humidade MÁXIMA";
                }
                if (message.value < HumiMin)
                {
                    return message.sensorid + " Menor que a humidade MINIMA";
                }
            }

            if (message.sensorid.ToUpper().Contains("LUMINOSITY"))
            {
                if (message.value > LumiMax)
                {
                    return message.sensorid + " Maior que a luminosidade MÁXIMA";
                }
                if (message.value < LumiMin)
                {
                    return message.sensorid + " Menor que a luminosidade MINIMA";
                }
            }


            return result;
        }
    }
}
