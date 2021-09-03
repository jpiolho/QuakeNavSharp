using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeNavSharp.Navigation
{
    public class NavigationGraphNodeList : IList<NavigationGraph.Node>
    {
        private readonly List<NavigationGraph.Node> _list = new List<NavigationGraph.Node>();

        public NavigationGraph.Node this[int index] { get => _list[index]; set { throw new NotSupportedException(); } }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(NavigationGraph.Node item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(NavigationGraph.Node item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(NavigationGraph.Node[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<NavigationGraph.Node> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(NavigationGraph.Node item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, NavigationGraph.Node item)
        {
            _list.Insert(index, item);
        }

        public bool Remove(NavigationGraph.Node item)
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
            // Delete all links to this node
            foreach(var node in _list)
            {
                for (var i = node.Links.Count - 1; i >= 0; i--)
                    if (node.Links[i].Target == node)
                        node.Links.RemoveAt(i);
            }

            // Subtract 1 from the id of all the nodes above this one
            for (var i = index+1; i < _list.Count; i++)
                _list[i].Id--;

            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }

        internal int Capacity { get => _list.Capacity; set => _list.Capacity = value; }
    }
}
