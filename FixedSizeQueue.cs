using System.Collections.Generic;

namespace GSLogger
{
    public class FixedSizeQueue<T>:Queue<T>
    {
        private readonly object _locker = new object();

        public int Limit { get; set; }
        new public void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (_locker)
            {
                while (Count > Limit) Dequeue();
            }
        }

        public FixedSizeQueue(int limit)
        {
            Limit = limit;
        }

    }
}
