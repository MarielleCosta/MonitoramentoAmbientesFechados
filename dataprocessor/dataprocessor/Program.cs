using dataprocessor.Models;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;
using System.Text.Json;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using dataprocessor.Services;
using MySql.Data.MySqlClient;

namespace dataprocessor
{
    public class Program
    {
        public static string server = "localhost";
        public static string port = "3306";
        public static string user = "admin";
        public static string password = "admin";
        public static string database = "SENSORS";
        

        public static HiveMQClientOptions options = new HiveMQClientOptions
        {
            Host = "5c4d923ba33549faa5f72062b1fac92e.s1.eu.hivemq.cloud",
            Port = 8883,
            UseTLS = true,
            UserName = "admin",
            Password = "YzRPJJ@g6cubt1",
            ConnectTimeoutInMs = 10000, // Aumentar o timeout para 10 segundos
        };

        public static ConcurrentQueue<InitialMessage> InitialMessagesReceived = new ConcurrentQueue<InitialMessage>();
        public static List<Thread> ThreadSensors = new List<Thread>();
        public static HiveMQClient client;
        public static List<TimeLastMessageReceived> timeLastMessageReceiveds = new List<TimeLastMessageReceived>();
        public static Limites limites;
        public static ConcurrentQueue<ReceivedMessage> ReceivedMessages = new ConcurrentQueue<ReceivedMessage>();
        public static ConcurrentQueue<Alert> Alerts = new ConcurrentQueue<Alert>();

        #region MYSQL
        static public async void TesteConnection()
        {
            try
            {
                string connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("Conectado ao mysql");

                    string sensorId = "sensor_1";
                    DateTime recordedTimestamp = DateTime.Now;
                    float measure = 25.5f;

                    string query = $"INSERT INTO TEMPERATURE (SENSOR_ID, RECORDED_TIMESTAMP, MEASURE) VALUES (\"{sensorId}\", \"{recordedTimestamp.ToString("yyyy-MM-dd hh:mm:ss")}\", 10.7);";
                    Console.WriteLine(query);

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static public async void MySQLRun()
        {
            Console.WriteLine("MySQLRun");
            MySQLService mySQLService = new MySQLService(server, port, user, password, database);
            
            while (true)
            {
                if (ReceivedMessages.TryDequeue(out ReceivedMessage message))
                {
                    string resultAlert = limites.VerifyLimits(message);

                    if(resultAlert != "")
                    {
                        Console.WriteLine("VALOR FORA DOS LIMITES");
                        Alerts.Enqueue(new Alert(message.timestamp, resultAlert, message.sensorid));
                    }

                    bool result = await mySQLService.InsertSensorData(message);

                    if (!result)
                    {
                        ReceivedMessages.Enqueue(message);
                    }
                }

                if (Alerts.TryDequeue(out Alert alert))
                {
                    bool result = await mySQLService.InsertSensorAlert(alert);

                    if (!result)
                    {
                        Alerts.Enqueue(alert);
                    }
                }
                Thread.Sleep(100);
            }
        }
        #endregion
        
        //Conecta ao Broker MQTT e cria um cliente
        public static async Task<bool> MQTTConnect()
        {
            client = new HiveMQClient(options);
            Console.WriteLine($"Connecting to {options.Host} on port {options.Port} ...");
            ConnectResult connectResult;

            try
            {
                connectResult = await client.ConnectAsync().ConfigureAwait(false);

                if (connectResult.ReasonCode == ConnAckReasonCode.Success)
                {
                    Console.WriteLine($"Connect successful : {connectResult}");
                }
                else
                {
                    Console.WriteLine($"Connect failed: {connectResult}");
                    return false; ;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection exception: {ex}");
                return false;
            }

            return true;
        }
        // Thread responsável por se inscrever no tópico principal "sensor_monitors"
        public static async void MQTTSensorMonitor()
        {
            await MQTTConnect();

            try
            {
                client.OnMessageReceived += async (sender, args) =>
                {
                    if(args.PublishMessage.Topic == "/sensor_monitors")
                    {
                        string message = args.PublishMessage.PayloadAsString;
                        Console.WriteLine($"Received message: {message}");

                        try
                        {
                            InitialMessage? receivedMessage = JsonSerializer.Deserialize<InitialMessage>(message);

                            if (receivedMessage != null)
                            {
                                InitialMessagesReceived.Enqueue(receivedMessage);
                            }
                        }
                        catch (Exception Ex)
                        {
                            Console.WriteLine($"Deserialization error: {Ex.Message}");
                            return;
                        }
                    }                    
                };

                await client.SubscribeAsync("/sensor_monitors").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection exception: {ex}");
                return;
            }
        }
        //Analisa as mensagens de inicialiazação e cria threads para escutar cada novo tópico
        public static void MQTTSensors()
        {
            while (true)
            {
                if (InitialMessagesReceived.TryDequeue(out InitialMessage initialMessage))
                {
                    foreach (var sensor in initialMessage.sensors)
                    {
                        string topic = $"/sensor_monitors/{initialMessage.machine_id}/{sensor.sensor_id}";
                        Console.WriteLine($"Processing sensor: {topic}");

                        if (!ThreadSensors.Any(a => a.Name == topic))
                        {
                            var sensorThread = new Thread(() => { MQTTReceivingSensorMessages(topic); });
                            ThreadSensors.Add(sensorThread);
                            sensorThread.Start();
                            sensorThread.Name = topic;


                            //Inicialização do objeto ultima mensagem recebida
                            TimeLastMessageReceived timeLast = new TimeLastMessageReceived();
                            timeLast.Topic = topic;
                            timeLast.LastReceived = DateTime.Now;
                            timeLast.Interval = sensor.data_interval;
                            timeLast.TimoutAlertSent = false;

                            timeLastMessageReceiveds.Add(timeLast);
                        }
                        else
                        {
                            timeLastMessageReceiveds.Where(a => a.Topic == topic).First().LastReceived = DateTime.Now;
                            timeLastMessageReceiveds.Where(a => a.Topic == topic).First().TimoutAlertSent = false;
                            Console.WriteLine($"Sensor thread for {topic} already exists.");
                        }
                    }
                }
            }
        }
        public static async void MQTTReceivingSensorMessages(string topic)
        {
            try
            {
                client.OnMessageReceived += async (sender, args) =>
                {
                    if(args.PublishMessage.Topic == topic)
                    {
                        string message = args.PublishMessage.PayloadAsString;
                        Console.WriteLine($"Thread {topic} - Received message:");
                        

                        try
                        {
                            ReceivedMessage? receivedMessage = JsonSerializer.Deserialize<ReceivedMessage>(message);

                            if (receivedMessage != null)
                            {
                                receivedMessage.sensorid = topic.Split("/").Last();
                                ReceivedMessages.Enqueue(receivedMessage);

                                receivedMessage.Show();
                            }
                        }
                        catch (Exception Ex)
                        {
                            Console.WriteLine($"Deserialization error: {Ex.Message}");
                            return;
                        }

                        //Atualiza o valor de timestamp da última mensagem recebida
                        if (timeLastMessageReceiveds.Where(t => t.Topic == topic).Count() > 0)
                        {
                            timeLastMessageReceiveds.Where(t => t.Topic == topic).First().LastReceived = DateTime.Now;
                            //timeLastMessageReceiveds.Where(t => t.Topic == topic).First().TimoutAlertSent = true;
                        }
                        else
                        {
                            Console.WriteLine("Error. Message received from a unknown sensor");
                        }
                    }
                };

                await client.SubscribeAsync(topic).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection exception: {ex}");
                return;
            }
        }
        public static void MQTTAlertIntervalVerifier()
        {
            while(true)
            {
                foreach(var last in timeLastMessageReceiveds)
                {
                    TimeSpan span = DateTime.Now - last.LastReceived;

                    if(span.TotalMilliseconds > 10*last.Interval && last.TimoutAlertSent == false)
                    {
                        Console.WriteLine($"Topic {last.Topic} - Timeout");
                        last.TimoutAlertSent = true;


                        Alert alert = new Alert();

                        alert.timestamp = DateTime.Now;
                        alert.message = "Timeout - mensagem não recebida em 10 intervalos especificados";
                        alert.sensorid = last.Topic.Split("/").Last();


                        Alerts.Enqueue( alert );
                    }
                }

                Thread.Sleep(100);
            }
        }
        static void Main()
        {
            limites = new Limites();

            Thread MqttInit = new Thread(MQTTSensorMonitor);
            MqttInit.Start();

            Thread MqttSensors = new Thread(MQTTSensors);
            MqttSensors.Start();

            Thread AlertVerifier = new Thread(MQTTAlertIntervalVerifier);
            AlertVerifier.Start();

            Thread MySQL = new Thread(MySQLRun);
            MySQL.Start();


            MySQL.Join();
            MqttSensors.Join();
            MqttInit.Join();
            AlertVerifier.Join();
        }
    }
}
