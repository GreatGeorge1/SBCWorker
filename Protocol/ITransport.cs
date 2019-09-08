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

    public interface ITransport<Ttype>
    {
        Task<bool> WriteMessageAsync(Ttype input);
        /// <summary>
        /// Читает сообщение и добавляет построчно в InputQueue
        /// </summary>
        Task ReadMessageAsync();
        bool Init();
        MessageQueue<Ttype> InputQueue { get; set; }
        MessageQueue<Ttype> OutputQueue { get; set; }
    }

    public interface IByteTransport:ITransport<byte[]>
    {
    }
}

