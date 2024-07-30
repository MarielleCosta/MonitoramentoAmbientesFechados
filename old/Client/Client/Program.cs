using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.Client.Results;
using System.Text.Json;
using Client.Models;

namespace DataProcessor
{

    public class Program
    {
        static public ReceivedMessagesQueue queue = new ReceivedMessagesQueue();
        static async Task ReceiveMessagesAsync()
        {
            HiveMQClientOptions options = new HiveMQClientOptions
            {
                Host = "5c4d923ba33549faa5f72062b1fac92e.s1.eu.hivemq.cloud",
                Port = 8883,
                UseTLS = true,
                UserName = "admin",
                Password = "YzRPJJ@g6cubt1",
            };

            var client = new HiveMQClient(options);

            Console.WriteLine($"Connecting to {options.Host} on port {options.Port} ...");

            ConnectResult connectResult;

            try
            {
                connectResult = await client.ConnectAsync().ConfigureAwait(false);

                if (connectResult.ReasonCode == ConnAckReasonCode.Success)
                {
                    Console.WriteLine($"Connect successful: {connectResult}");
                }
                else
                {
                    Console.WriteLine($"Connect failed: {connectResult}");
                    Environment.Exit(-1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }

            client.OnMessageReceived += (sender, args) =>
            {
                string message = args.PublishMessage.PayloadAsString;
                ReceivedMessage receivedMessage = JsonSerializer.Deserialize<ReceivedMessage>(message);
                queue.InsertInQueue(receivedMessage);
            };

            await client.SubscribeAsync("hivemqdemo/telemetry").ConfigureAwait(false);


            while (true) { }
            // Keep the application running to receive messages
            //Console.WriteLine("Subscribed to hivemqdemo/telemetry. Press Enter to exit.");
            //Console.ReadLine();
        }

        static void Main()
        {
            Task.Run(ReceiveMessagesAsync);

            Task.Run(() =>
            {
                while (true)
                {
                    queue.ShowQueue();
                    Thread.Sleep(1000);
                }
            });
            Console.ReadKey();
        }

    }
}