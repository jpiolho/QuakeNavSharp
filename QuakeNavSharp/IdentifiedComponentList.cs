using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Navigation
{
    public class IdentifiedComponentList<T> : IList<T> where T : IdentifiedComponentBase
    {
        private readonly List<T> _list;

        internal delegate void ListEventHandler(object sender, int index, T obj);
        internal event ListEventHandler Removing;

        public IdentifiedComponentList()
        {
            _list = new List<T>();
        }
        public IdentifiedComponentList(int capacity) : this()
        {
            this.Capacity = capacity;
        }

        public T this[int index] { get => _list[index]; set { throw new NotSupportedException(); } }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(T item) => _list.Add(item);

        public void Clear() => _list.Clear();
        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item) => _list.Insert(index, item);

        public bool Remove(T item)
        {
            var nodeIdx = IndexOf(item);
            if (nodeIdx == -1)
                return false;

            // Do RemoveAt so that the ID logic applies
            _list.RemoveAt(nodeIdx);

            return true;
        }

        public void RemoveAt(int index)
        {
            Removing?.Invoke(this, index, _list[index]);
            
            // Subtract 1 from the id of all the items above this one
            for (var i = index + 1; i < _list.Count; i++)
                _list[i].Id--;

            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

        internal int Capacity { get => _list.Capacity; set => _list.Capacity = value; }
    }
}
