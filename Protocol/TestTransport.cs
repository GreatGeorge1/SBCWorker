using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public class TestTransport : ITransport
    {
        public TestTransport()
        {
            InputQueue = new MessageQueue<string>();
        }

        public MessageQueue<string> InputQueue { get; set; }

        public bool Init()
        {
            Console.WriteLine("Test transport init");
            return true;
        }

        public Task ReadMessageAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteMessageAsync(string input)
        {
            InputQueue.Enqueue(input);
            return true;
        }
    }
}
