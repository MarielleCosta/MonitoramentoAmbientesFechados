using dataprocessor.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dataprocessor.Services
{
    public class MySQLService
    {
        // Definindo os parâmetros de conexão
        public string server { get; set; }
        public string port { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string database { get; set; }

        public MySQLService(string _server, string _port, string _user, string _pass, string _db)
        {
            server = _server;
            port = _port;
            user = _user;
            password = _pass;
            database = _db;
        }

        public async Task<bool> InsertSensorData(string table, string sensorId, DateTime recordedTimestamp, float measure)
        {
            try
            {
                // Criando a string de conexão
                string connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = $"INSERT INTO {table} (SENSOR_ID, RECORDED_TIMESTAMP, MEASURE) VALUES " +
                                $"(\"{sensorId}\"," +
                                $" \"{recordedTimestamp.ToString("yyyy-MM-dd hh:mm:ss")}\", " +
                                $"{measure.ToString().Replace(",", ".")});";
                    
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine("Inserção realizada");
                return true;
            }catch (Exception ex)
            {
                Console.WriteLine("Deu ruim");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> InsertSensorData(ReceivedMessage message)
        {
            string table = "";
            if(message == null)
            {
                return false;
            }

            if (message.sensorid.ToUpper().Contains("TEMPERATURE"))
            {
                table = "TEMPERATURE";
            }
            if (message.sensorid.ToUpper().Contains("HUMIDITY"))
            {
                table = "HUMIDITY";
            }
            if (message.sensorid.ToUpper().Contains("LUMINOSITY"))
            {
                table = "LUMINOSITY";
            }

            try
            {
                // Criando a string de conexão
                string connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = $"INSERT INTO {table} (SENSOR_ID, RECORDED_TIMESTAMP, MEASURE) VALUES " +
                                $"(\"{message.sensorid}\"," +
                                $" \"{message.timestamp.ToString("yyyy-MM-dd HH:mm:ss")}\", " +
                                $"{message.value.ToString().Replace(",", ".")});";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine("Inserção realizada");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Inserção não foi realizada");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        internal async Task<bool> InsertSensorAlert(Alert alert)
        {
            string table = "";
            if (alert == null)
            {
                return false;
            }

            if (alert.sensorid.ToUpper().Contains("TEMPERATURE"))
            {
                table = "TEMPERATURE_ALERTS";
            }
            if (alert.sensorid.ToUpper().Contains("HUMIDITY"))
            {
                table = "HUMIDITY_ALERTS";
            }
            if (alert.sensorid.ToUpper().Contains("LUMINOSITY"))
            {
                table = "LUMINOSITY_ALERTS";
            }

            try
            {
                // Criando a string de conexão
                string connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = $"INSERT INTO {table} (SENSOR_ID, RECORDED_TIMESTAMP, ALERT) VALUES " +
                                $"('{alert.sensorid}'," +
                                $" '{alert.timestamp.ToString("yyyy-MM-dd HH:mm:ss")}', " +
                                $"'{alert.message.ToString().Replace(",", ".")}');";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine("Inserção realizada");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Inserção não foi realizada");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
