using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class ReceivedMessagesQueue
    {
        private List<ReceivedMessage> _ReceivedMessagesQueue = new List<ReceivedMessage>();
        public object _lock = new object();

        public bool InsertInQueue(ReceivedMessage Message)
        {
            lock (_lock)
            {
                _ReceivedMessagesQueue.Add(Message);
            }

            return false;
        }
        public bool ShowQueue()
        {
            lock (_lock)
            {
                Console.WriteLine("Mensagens");
                foreach (var Message in _ReceivedMessagesQueue)
                {
                    Console.WriteLine("Temperatura: " + Message.temperature);
                    Console.WriteLine("Humidade: " + Message.humidity);
                }
            }

            return false;
        }

    }
}
