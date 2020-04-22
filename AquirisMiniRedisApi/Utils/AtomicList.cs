using System.Collections;
using System.Collections.Generic;

namespace AquirisMiniRedisApi.Utils
{
//simple atomic list from stackoverflow example:
//https://stackoverflow.com/questions/11085444/atomic-list-is-this-collection-threadsafe-and-fast
    public class AtomicList<T> : IEnumerable<T>
    {
        private readonly object _locker = new object();
        private List<T> _internalCollection = new List<T>();

        public void Add(T value)
        {
            lock (_locker)
            {
                List<T> update = new List<T>(_internalCollection) {value};
                _internalCollection = update;
            }
        }

        public void Remove(T value)
        {
            lock (_locker)
            {
                List<T> update = new List<T>(_internalCollection);
                update.Remove(value);
                _internalCollection = update;
            }
        }

        public void RemoveAll(IEnumerable<T> value)
        {
            lock (_locker)
            {
                List<T> update = new List<T>(_internalCollection);
                foreach (var val in value)
                {
                    update.Remove(val); 
                }
                _internalCollection = update;
            }
        }
        public void Replace(IEnumerable<T> range)
        {
            lock (_locker)
            {
                List<T> update = new List<T>();
                update.AddRange(range);
                _internalCollection = update;
            }
        }
        public IEnumerator<T> GetEnumerator() => _internalCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
