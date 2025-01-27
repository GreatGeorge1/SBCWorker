﻿using Microsoft.Extensions.Hosting;
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

    public interface ITransport<TType>
    {
        Task<bool> WriteMessageAsync(TType input);
        /// <summary>
        /// Читает сообщение и добавляет построчно в InputQueue
        /// </summary>
        Task ReadMessageAsync();
        bool Init();
        ConcurrentMessageBag<TType> InputQueue { get; }
        ConcurrentMessageBag<TType> OutputQueue { get; }
        string GetInfo();
    }

    public interface IByteTransport:ITransport<byte[]>
    {
    }
}

