using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Protocol
{
    //public class TestTransport : IByteTransport
    //{
    //    public TestTransport()
    //    {
    //        InputQueue = new ConcurrentMessageBag<byte[]>();
    //    }

    //    public ConcurrentMessageBag<byte[]> InputQueue { get; private set; }
    //    public ConcurrentMessageBag<byte[]> OutputQueue { get; private set; }

    //    public string GetInfo()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool Init()
    //    {
    //        Console.WriteLine("Test transport init");
    //        return true;
    //    }

    //    public void ReadMessage()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public async Task<bool> WriteMessage(byte[] input)
    //    {
    //        InputQueue.Enqueue(input);
    //        return true;
    //    }
    //}
}
