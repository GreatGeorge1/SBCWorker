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
        public readonly ConcurrentDictionary<string, ConcurrentMessageBag<SignalRMessage>> Dictionary;

        public InputMessageQueue(IEnumerable<SerialConfig> ports)
        {
            Dictionary = new ConcurrentDictionary<string, ConcurrentMessageBag<SignalRMessage>>();
            foreach (var port in ports)
            {
                var tempQueue = new ConcurrentMessageBag<SignalRMessage>
                {
                    Port = port.PortName
                };
                Dictionary.TryAdd(port.PortName, tempQueue); 
            }
        }
    }

    public class OutputMessageQueue : IMessageQueue
    {
        public readonly ConcurrentDictionary<string, ConcurrentMessageBag<SignalRresponse>> Dictionary;

        public OutputMessageQueue(IEnumerable<SerialConfig> ports)
        {
            Dictionary = new ConcurrentDictionary<string, ConcurrentMessageBag<SignalRresponse>>();
            foreach (var port in ports)
            {
                var tempQueue = new ConcurrentMessageBag<SignalRresponse>
                {
                    Port = port.PortName
                };
                Dictionary.TryAdd(port.PortName, tempQueue);
            }
        }
    }
}
