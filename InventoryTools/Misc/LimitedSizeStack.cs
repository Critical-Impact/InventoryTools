using System.Collections.Generic;

namespace InventoryTools.Misc
{
    public class LimitedSizeStack<T> : LinkedList<T>
    {
        private readonly int _maxSize;
        public LimitedSizeStack(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Push(T item)
        {
            this.AddFirst(item);

            if(this.Count > _maxSize)
                this.RemoveLast();
        }

        public T? Pop()
        {
            if (First != null)
            {
                var item = First.Value;
                this.RemoveFirst();
                return item;
            }

            return default;
        }
    }
}