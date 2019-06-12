using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Host
{
    public class CustomQueue<T>:Queue<T>
    {
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            OnEnqueueEvent(item);
        }
        public event CustomQueueEnqueueEventHandler EnqueueEvent;

        protected void OnEnqueueEvent(T item)
        {
            EnqueueEvent?.Invoke(this, new CustomQueueEnqueueEventArgs<T>(item));
        }
        public delegate void CustomQueueEnqueueEventHandler(object sender, CustomQueueEnqueueEventArgs<T> e);
      
    }
    public class CustomQueueEnqueueEventArgs<T>
    {
        public CustomQueueEnqueueEventArgs(T item)
        {
            Item = item;
        }

        public T Item { get; set; }
    }

}
