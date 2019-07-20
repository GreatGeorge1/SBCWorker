using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol
{
    public class MessageQueue<T>:Queue<T>
    {
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            OnEnqueueEvent(item);
        }
        public event CustomQueueEnqueueEventHandler EnqueueEvent;

        protected void OnEnqueueEvent(T item)
        {
            EnqueueEvent?.Invoke(this, new MessageQueueEnqueueEventArgs<T>(item));
        }
        public delegate void CustomQueueEnqueueEventHandler(object sender, MessageQueueEnqueueEventArgs<T> e);
      
    }
    public class MessageQueueEnqueueEventArgs<T>
    {
        public MessageQueueEnqueueEventArgs(T item)
        {
            Item = item;
        }

        public T Item { get; set; }
    }

}
