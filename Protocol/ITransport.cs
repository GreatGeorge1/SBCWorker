using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Protocol
{
    public interface ITransport:ITransport<string>
    {
     //   Task<bool> WriteMessageAsync(string input);
        /// <summary>
        /// Читает сообщение и добавляет построчно в InputQueue
        /// </summary>
       // Task ReadMessageAsync();
        //bool Init();
        //MessageQueue<string> InputQueue { get; set; }
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
    }

    public interface IByteTransport:ITransport<byte[]>
    {
       // Task<bool> WriteMessageAsync(byte[] input);
        /// <summary>
        /// Читает сообщение и добавляет построчно в InputQueue
        /// </summary>
       // Task ReadMessageAsync();
       // bool Init();
       // MessageQueue<byte[]> InputQueue { get; set; }
    }
}

