using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Protocol
{
    public interface ITransport
    {
        Task<bool> WriteMessageAsync(string input);
        /// <summary>
        /// Читает сообщение и добавляет построчно в InputQueue
        /// </summary>
        Task ReadMessageAsync();
        bool Init();
        MessageQueue<string> InputQueue { get; set; }
    }
}
