﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol
{
    public class MessageQueue<T>:Queue<T>, ICollection<T>
    {
        public string Port { get; set; }//TODO govno

        public bool IsReadOnly => false;

        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            OnEnqueueEvent(item);
        }
        public event CustomQueueEnqueueEventHandler EnqueueEvent;

        protected void OnEnqueueEvent(T item)
        {
            //EnqueueEvent?.Invoke(this, new MessageQueueEnqueueEventArgs<T>(item));
            EnqueueEvent?.Invoke(this, new MessageQueueEnqueueEventArgs<T>(item, Port));
        }

        public void Add(T item)
        {
            base.Enqueue(item);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public delegate void CustomQueueEnqueueEventHandler(object sender, MessageQueueEnqueueEventArgs<T> e);
      
    }
    public class MessageQueueEnqueueEventArgs<T>
    {
        public MessageQueueEnqueueEventArgs(T item)
        {
            Item = item;
        }

        public MessageQueueEnqueueEventArgs(T item, String port)
        {
            Item = item;
            Port = port;
        }

        public string Port { get; set; }
        public T Item { get; set; }
    }

}
