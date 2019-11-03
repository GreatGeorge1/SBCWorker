using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Protocol
{
    public class TestTransport : IByteTransport
    {
        public TestTransport()
        {
            InputQueue = new MessageQueue<byte[]>();
        }

        public MessageQueue<byte[]> InputQueue { get; set; }
        public MessageQueue<byte[]> OutputQueue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string GetInfo()
        {
            throw new NotImplementedException();
        }

        public bool Init()
        {
            Console.WriteLine("Test transport init");
            return true;
        }

        public Task ReadMessageAsync()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteMessageAsync(byte[] input)
        {
            InputQueue.Enqueue(input);
            return true;
        }
    }
}
