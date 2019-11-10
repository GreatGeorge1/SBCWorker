using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Worker.Host.SignalR
{
    interface IMessageQueue
    {
    }

    public class InputMessageQueue : IMessageQueue
    {
        public readonly ConcurrentDictionary<string, MessageQueue<SignalRMessage>> Dictionary;

        public InputMessageQueue(IEnumerable<SerialConfig> ports)
        {
            Dictionary = new ConcurrentDictionary<string, MessageQueue<SignalRMessage>>();
            foreach (var port in ports)
            {
                var tempQueue = new MessageQueue<SignalRMessage>
                {
                    Port = port.PortName
                };
                Dictionary.TryAdd(port.PortName, tempQueue); 
            }
        }
    }

    public class OutputMessageQueue : IMessageQueue
    {
        public readonly ConcurrentDictionary<string, MessageQueue<SignalRresponse>> Dictionary;

        public OutputMessageQueue(IEnumerable<SerialConfig> ports)
        {
            Dictionary = new ConcurrentDictionary<string, MessageQueue<SignalRresponse>>();
            foreach (var port in ports)
            {
                var tempQueue = new MessageQueue<SignalRresponse>
                {
                    Port = port.PortName
                };
                Dictionary.TryAdd(port.PortName, tempQueue);
            }
        }
    }
}
