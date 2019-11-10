using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Protocol
{
    public interface ITransport:ITransport<string>
    {
    }

    public interface ITransport<TType>:IHostedService
    {
        Task<bool> WriteMessageAsync(TType input);
        /// <summary>
        /// Читает сообщение и добавляет построчно в InputQueue
        /// </summary>
        void ReadMessage();
        bool Init();
        MessageQueue<TType> InputQueue { get; }
        MessageQueue<TType> OutputQueue { get; }
        string GetInfo();
    }

    public interface IByteTransport:ITransport<byte[]>
    {
    }
}

